#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>
#include <pthread.h>

#include "Client.h"
#include "Packet.h"
#include "ServerSettings.h"
#include "Math.h"
#include "SaveFile.h"

#include "Generation.cpp"
#pragma comment (lib, "ws2_32.lib")

const int max_threads = 64;
const int thread_size = 64;
const int port = 26951;

const int recv_buffer = 4096;
const int send_buffer = 16384;

int currentThreadCount = 0;
bool running = true;

std::map<SOCKET, Client> clients;
timeval select_interval;

GenerateData generate_data = GenerateData(true);
GenerateData temp_gd = GenerateData(false);

#pragma region Send

void launcherData_send(Client client, int m_version, int c_version, int b_version)
{
	Packet packet = Packet();
	packet.Write((uint8_t)2);

	bool update = false;
	if (m_version != 1) update = true;
	if (c_version != 1) update = true;
	if (b_version != 1) update = true;
	packet.Write(update);

	if (update)
	{
		packet.Write("Proof Of Concept");
		packet.Write(1);
		packet.Write(1);
		packet.Write(1);
	}

	packet.Write(0); // Todo : Add patch notes & news here
	send(client.socket, packet.buffer, send_buffer, 0);
}

void backend_preview_send(Client client, uint8_t size)
{
	if (!client.is_backend) return;

	size = Clamp_8(size, 0, 100);
	int i = 0;

	Packet packet = Packet();
	packet.Write((uint8_t)12);

	packet.Write(size);
	packet.Write(temp_gd.chunk_size);

	for (uint8_t x = 0; x < size; x++)
	{
		for (uint8_t y = 0; y < size; y++)
		{
			packet.Write(x);
			packet.Write(y);
			
			uint16_t* heightmap = GenerateHeightmap(temp_gd, x, y, 0);
			int n = 0;

			for (uint8_t z = 0; z < temp_gd.chunk_size; z++)
			{
				for (uint8_t w = 0; w < temp_gd.chunk_size; w++)
				{
					packet.Write(heightmap[n]);
					n++;
				}
			}
		}
	}

	send(client.socket, packet.buffer, send_buffer, 0);
}

#pragma endregion
#pragma region Handle

void on_connect_handle(Client client, Packet packet)
{
	std::cout << "Client " << client.client_id << "has authenticated its connection." << endl;
	client.connection_state = 1;
}

void launcher_data_handle(Client client, Packet packet)
{
	launcherData_send(client, packet.ReadInt(), packet.ReadInt(), packet.ReadInt());
}

void backend_login_handle(Client client, Packet packet)
{
	if (packet.ReadString() == "11ac1c39-1957-44c0-aa3e-2ed005581592" &&
		packet.ReadString() == "PragmaticMilan85$")
	{
		cout << "Client " << client.client_id << " is now a backend client" << endl;

		Packet sendPacket = Packet();
		sendPacket.Write((uint8_t)11);
		sendPacket.Write(true);

		generate_data.Serialize(sendPacket);
		send(client.socket, sendPacket.buffer, send_buffer, 0);

		client.is_backend = true;
	}
	else
	{
		cout << "Client " << client.client_id << " has tried and failed to login as a backend client. Kicking" << endl;

		// UN IMPLEMENTED
	}
}

void backend_preview_handle(Client client, Packet packet)
{
	if (!client.is_backend) return;

	temp_gd = GenerateData(packet);
	backend_preview_send(client, packet.ReadChar());
}

void backend_save_handle(Client client, Packet packet)
{
	if (temp_gd.initialized && client.is_backend)
	{
		generate_data = temp_gd;

		Packet packet = Packet();
		generate_data.Serialize(packet);

		FILE* stream;
		int response = fopen_s(&stream, "generateData.txt", "wb");

		if (response == 0)
		{
			fwrite(packet.buffer, 1, packet.readPos, stream);
		}
		else
		{
			std::cout << "Failed to write new generate data to file!" << std::endl;
		}

		if (stream) fclose(stream);
	}
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
			Packet packet = Packet();

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
				case 1: on_connect_handle(clients[sock], packet); break; // On Connect
				case 2: launcher_data_handle(clients[sock], packet); break; // Launcher Data
				case 13: backend_login_handle(clients[sock], packet); break; // Backend Login
				case 14: backend_preview_handle(clients[sock], packet); break; // Backend Preview
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
	FILE* stream;
	int response = fopen_s(&stream, "generateData.txt", "rb");

	if (response == 0)
	{
		Packet packet = Packet();
		fread(packet.buffer, 1, send_buffer, stream);

		if (ftell(stream) < 5)
		{
			cout << "No generate data available, using defaults" << endl;
		}
	}
	else
	{
		std::cout << "Failed to write new generate data to file!" << std::endl;
	}

	if (stream) fclose(stream);

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

			Packet packet = Packet();
			packet.Write((uint8_t)1);
			packet.Write((int)clients.size());
			send(socket, packet.buffer, 4096, 0);

			Client client = Client(clients.size(), socket);
			clients.insert(std::make_pair(socket, client));

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