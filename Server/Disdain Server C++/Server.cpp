#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>
#include <pthread.h>

#include "Client.h"
#include "Packet.h"

#pragma comment (lib, "ws2_32.lib")

using namespace std;

const int max_threads = 64;
const int thread_size = 64;
const int port = 26951;

const int recv_buffer = 4096;
const int send_buffer = 16384;

const string client_typeVersion = "Closed Proof Of Concept";
const int client_mainVersion = 1;
const int client_contentVersion = 1;
const int client_bugfixVersion = 1;

int currentThreadCount = 0;
bool running = true;

std::map<SOCKET, Client> clients;
timeval select_interval;

#pragma region Send

void onConnect_send(Client client)
{
	Packet packet = Packet(true, 1);
	packet.Write(client.client_id);
	send(client.socket, packet.buffer, packet.readPos, 0);
}

void launcherData_send(Client client, int m_version, int c_version, int b_version)
{
	Packet packet = Packet(true, 2);

	bool update = false;
	if (m_version != client_mainVersion) update = true;
	if (c_version != client_contentVersion) update = true;
	if (b_version != client_bugfixVersion) update = true;
	packet.Write(update);

	if (update)
	{
		packet.Write(client_typeVersion);
		packet.Write(client_mainVersion);
		packet.Write(client_contentVersion);
		packet.Write(client_bugfixVersion);
	}

	packet.Write(0); // Todo : Add patch notes & news here
	std::cout << "pls" << std::endl;
	send(client.socket, packet.buffer, 4096, 0);
}

#pragma endregion

#pragma region Handle

void onConnect_handle(Client client, Packet packet)
{
	std::cout << "Client " << client.client_id << "has authenticated its connection." << endl;
	client.connection_state = 1;
}

void launcherData_handle(Client client, Packet packet)
{
	launcherData_send(client, packet.ReadInt(), packet.ReadInt(), packet.ReadInt());
}

#pragma endregion

#pragma region Thread Functions

struct ThreadData
{
	bool in_use = false;
	int id = -1;
	int player_count = 0;

	std::map<SOCKET, int> sockets;
	Client clients[64];
	Client incoming_client;
};

void* socket_thread(void* arg)
{
	// Thread startup
	struct ThreadData* data;
	data = (struct ThreadData *) arg;

	data->incoming_client.thread_id = 0;
	data->clients[0] = data->incoming_client;
	data->sockets.insert(std::make_pair(data->incoming_client.socket, 0));
	data->incoming_client = Client();

	data->player_count = 1;
	data->in_use = true;
	fd_set readfds;

	while (true)
	{
		// Check whether to keep alive
		if (data->player_count == 0 &&
			!data->incoming_client.initialized)
		{
			std::cout << "Thread " << data->id << " stopping due to no players in thread" << std::endl;
			break;
		}

		if (data->incoming_client.initialized && data->player_count < thread_size)
		{
			for (int i = 0; i < thread_size; i++)
			{
				if (!data->clients[i].initialized)
				{
					data->incoming_client.thread_id = i;
					data->clients[i] = data->incoming_client;
					data->sockets.insert(std::make_pair(data->incoming_client.socket, i));

					data->incoming_client = Client();
					data->player_count++;
				}
			}
		}
		FD_ZERO(&readfds);
		for (int i = 0; i < 64; i++)
		{
			if (data->clients[i].initialized)
			{
				FD_SET(data->clients[i].socket, &readfds);
			}
		}

		int socketCount = select(0, &readfds, nullptr, nullptr, nullptr);
		for (int i = 0; i < socketCount; i++)
		{
			SOCKET sock = readfds.fd_array[i];
			Packet packet = Packet(false, 0);

			int bytesIn = recv(sock, packet.buffer, 4096, 0);
			if (bytesIn <= 0) 
			{
				std::cout << "Client " << data->clients[sock].client_id << " has disconnected" << endl;

				clients.erase(sock);
				data->clients[data->sockets[sock]] = Client();
				data->sockets.erase(sock);
				data->player_count--;

				closesocket(sock);
			}
			else
			{
				switch (packet.ReadInt())
				{
				case 1: onConnect_handle(clients[sock], packet); break; // OnConnect
				case 2: launcherData_handle(clients[sock], packet); break; // Launcher Data
				}
			}
		}
	}
	data->in_use = false;
	data->id = false;
	data->player_count = 0;

	memset(&data->clients, 0, sizeof(data->clients));
	data->incoming_client = Client();

	pthread_exit(nullptr);
	return 0;
}

#pragma endregion

void main()
{
 	pthread_t threads[max_threads];
	ThreadData thread_data[max_threads];

	select_interval.tv_usec = 33;

	WSADATA wsData;
	WORD ver = MAKEWORD(2, 2);

	if (WSAStartup(ver, &wsData) != 0) { std::cerr << "Can't Initialize winsock! Quitting" << std::endl; return; }

	SOCKET listening = socket(AF_INET, SOCK_STREAM, 0);
	if (listening == INVALID_SOCKET) { std::cerr << "Can't create a socket! Quitting" << std::endl; return; }

	sockaddr_in hint;
	hint.sin_family = AF_INET;
	hint.sin_port = htons(26951);
	hint.sin_addr.S_un.S_addr = INADDR_ANY;

	bind(listening, (sockaddr*)&hint, sizeof(hint));
	listen(listening, SOMAXCONN);

	fd_set master;
	while (running)
	{
		FD_ZERO(&master);
		FD_SET(listening, &master);

		int socketCount = select(0, &master, nullptr, nullptr, nullptr);
		if (socketCount >= 0)
		{
			std::cout << "Incoming connection..." << std::endl;

			SOCKET socket = accept(listening, nullptr, nullptr);

			Packet packet = Packet(true, 1);
			packet.Write((int)clients.size());
			send(socket, packet.buffer, 4096, 0);

			Client client = Client(clients.size(), socket);
			clients.insert(std::make_pair(socket, client));

			
			//onConnect_send(client);

			std::cout << "Client " << client.client_id << " has been connected. Adding to a thread..." << std::endl;

			bool inThread = false;
			int thread_loop_count = 0;
			while (!inThread && thread_loop_count < currentThreadCount)
			{
				if (thread_data[thread_loop_count].player_count > 0
					&& thread_data[thread_loop_count].player_count < 64
					&& !thread_data[thread_loop_count].incoming_client.initialized)
				{
					std::cout << "Client " << client.client_id << " has been added to thread - '" <<
						thread_loop_count << "'.";

					thread_data[thread_loop_count].incoming_client = client;
					inThread = true;
				}
				thread_loop_count++;
			}

			if (!inThread)
			{
				int newThread_id = 0;

				for (int i = 0; i < max_threads; i++)
				{
					if (!thread_data[i].in_use)
					{
						newThread_id = i;
						break;
					}
				}

				thread_data[newThread_id] = ThreadData();
				thread_data[newThread_id].id = currentThreadCount;
				thread_data[newThread_id].incoming_client = client;

				int response = pthread_create(&threads[currentThreadCount], nullptr,
					socket_thread, (void*)&thread_data[newThread_id]);

				std::cout << "No thread available, creating thread " << currentThreadCount << " for client "
					<< client.client_id << "." << std::endl;
			}
		}
	}

	FD_CLR(listening, &master);
	closesocket(listening);

	while (master.fd_count > 0)
	{
		SOCKET sock = master.fd_array[0];

		FD_CLR(sock, &master);
		closesocket(sock);
	}

	WSACleanup();
	system("pause");
}