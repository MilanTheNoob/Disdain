#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>
#include <pthread.h>

#include "client.h"
#include "packet.h"
#include "generate_data.h"

#include "math.h"
#include "send.h"
#include "threading.h"

#pragma comment (lib, "ws2_32.lib")
std::map<SOCKET, client*> clients;

generate_data main_gd = generate_data(true);
generate_data temp_gd = generate_data(false);

std::vector<pthread_t*> threads;
std::vector<thread_data> thread_info;

void main()
{
	FILE* stream;
	int response = fopen_s(&stream, "generate_data.txt", "rb");

	if (response == 0)
	{
		packet recv_packet = packet(0, 819200);
		fread(recv_packet.buffer, 1, 819200, stream);

		if (ftell(stream) < 5)
		{
			std::cout << "No generate data available, using defaults" << std::endl;

		}
	}
	else
		std::cout << "Failed to read generate data from save file" << std::endl;

	if (stream) fclose(stream);

	WSADATA wsData;
	WORD ver = MAKEWORD(2, 2);

	if (WSAStartup(ver, &wsData) != 0) { std::cerr << "\nCan't Initialize winsock! Quitting" << std::endl; return; }

	SOCKET listening = socket(AF_INET, SOCK_STREAM, 0);
	if (listening == INVALID_SOCKET) { std::cerr << "Can't create a socket! Quitting" << std::endl; return; }

	sockaddr_in hint;
	hint.sin_family = AF_INET;
	hint.sin_port = htons(26951);
	hint.sin_addr.S_un.S_addr = INADDR_ANY;

	bind(listening, (sockaddr*)&hint, sizeof(hint));
	listen(listening, SOMAXCONN);

	fd_set master;
	while (true)
	{
		FD_ZERO(&master);
		FD_SET(listening, &master);

		int socketCount = select(0, &master, nullptr, nullptr, nullptr);
		if (socketCount >= 0)
		{
			std::cout << "\nIncoming connection..." << std::endl;

			SOCKET socket = accept(listening, nullptr, nullptr);

			client new_client = client(clients.size(), socket);
			clients.insert(std::make_pair(socket, &new_client));

			on_connect_send(clients.size(), &socket);
			std::cout << "Client " << new_client.client_id << " has been connected. Adding to a thread..." << std::endl;

			bool inThread = false;
			int thread_loop_count = 0;

			while (!inThread && thread_loop_count < threads.size())
			{
				if (thread_info[thread_loop_count].player_count + thread_info[thread_loop_count].incoming_clients.size() < 64)
				{
					std::cout << "Client " << new_client.client_id << " has been added to thread - '" <<
						thread_loop_count << "'." << std::endl;

					thread_info[thread_loop_count].incoming_clients.push_back(&new_client);
					inThread = true;
				}
				thread_loop_count++;
			}

			if (!inThread)
			{
				int new_thread_id = thread_info.size();
				thread_info.push_back(thread_data());

				thread_info[new_thread_id].incoming_clients.push_back(&new_client);
				thread_info[new_thread_id].all_clients = &clients;

				thread_info[new_thread_id].main_gd = &main_gd;
				thread_info[new_thread_id].temp_gd = &temp_gd;

				thread_info[new_thread_id].thread_holder = &threads;
				thread_info[new_thread_id].thread_data_holder = &thread_info;

				pthread_t thr;
				int response = pthread_create(&thr, nullptr, game_thread, (void*)&thread_info[new_thread_id]);
				threads.push_back(&thr);

				std::cout << "No thread available, creating thread " << threads.size() - 1 << " for client "
					<< new_client.client_id << "." << std::endl;
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