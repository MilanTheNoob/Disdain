#ifndef THREADING_H
#define THREADING_H

#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>
#include <pthread.h>

#include "client.h"
#include "packet.h"

#include "handle.h"
#pragma comment (lib, "ws2_32.lib")

struct thread_data
{
	int id = -1;
	int player_count = 0;

	std::map<SOCKET, client*>* all_clients;
	std::map<SOCKET, client*> clients;

	std::vector<SOCKET> sockets;
	std::vector<client*> incoming_clients;

	std::vector<thread_data>* thread_data_holder;
	std::vector<pthread_t*>* thread_holder;

	generate_data* main_gd;
	generate_data* temp_gd;
};

void* game_thread(void* arg)
{
	struct thread_data* data;
	data = (struct thread_data*)arg;

	for (int i = 0; i < data->incoming_clients.size(); i++)
	{
		if (data->incoming_clients[i] != nullptr)
		{
			data->incoming_clients[i]->thread_id = data->id;
			data->clients.insert(std::make_pair(data->incoming_clients[i]->socket, data->incoming_clients[i]));
			data->sockets.push_back(data->incoming_clients[i]->socket);

			data->player_count++;
		}
	}
	data->incoming_clients.clear();
	fd_set readfds;

	while (true)
	{
		/*
		if (data->player_count == 0 && data->incoming_client == nullptr)
		{
			std::cout << "\nThread " << data->id << " stopping due to no clients in thread" << std::endl;
			break;
		}
		*/

		if (data->incoming_clients.size() > 0)
		{
			for (int i = 0; i < data->incoming_clients.size(); i++)
			{
				data->incoming_clients[i]->thread_id = data->id;
				data->clients.insert(std::make_pair(data->incoming_clients[i]->socket, data->incoming_clients[i]));
				data->sockets.push_back(data->incoming_clients[i]->socket);

				data->player_count++;
			}
			data->incoming_clients.clear();
		}

		FD_ZERO(&readfds);
		for (int i = 0; i < data->sockets.size(); i++)
			FD_SET(data->sockets[i], &readfds);

		int socketCount = select(0, &readfds, nullptr, nullptr, nullptr);
		for (int i = 0; i < socketCount; i++)
		{
			SOCKET sock = readfds.fd_array[i];
			char* recv_buffer = new char[4096];

			int bytesIn = recv(sock, recv_buffer, 4096, 0);
			if (bytesIn <= 0)
			{
				std::cout << "Client " << data->clients[sock]->client_id << " has disconnected" << std::endl;

				for (int j = 0; j < data->sockets.size(); j++) 
				{
					if (data->sockets[j] == sock)
						data->sockets.erase(data->sockets.begin() + j);
				}

				data->all_clients->erase(sock);
				data->clients.erase(sock);
				data->player_count--;

				closesocket(sock);
			}
			else
			{
				int recv_packet_id = 0;
				bool second_value = false;

				memcpy(&second_value, &recv_buffer[5], 1);
				memcpy(&recv_packet_id, &recv_buffer[0], 4);

				client* recv_client = data->clients[sock];
				bool found_packet = false;

				for (int j = 0; j < recv_client->recving_packets.size(); j++)
				{
					if (recv_client->recving_packets[j].packet_id == recv_packet_id)
					{
						recv_client->recving_packets[j].add_chunk(recv_buffer);

						if (second_value)
							packet_handler(recv_client, &recv_client->recving_packets[j], recv_packet_id,
								data->main_gd, data->temp_gd);

						found_packet = true;
					}
				}

				if (!found_packet)
				{
					packet recv_packet = packet(recv_buffer);
					if (!second_value)
					{
						recv_client->recving_packets.push_back(recv_packet);
					}
					else
					{
						packet_handler(recv_client, &recv_packet, recv_packet_id,
							data->main_gd, data->temp_gd);
					}
				}
			}

			delete[] recv_buffer;
		}
	}

	data->id = false;
	data->player_count = 0;
	
	for (int i = 0; i < data->thread_data_holder->size(); i++)
	{
		if (data == &(*data->thread_data_holder)[i])
		{
			data->thread_data_holder->erase(data->thread_data_holder->begin() + i);
			data->thread_holder->erase(data->thread_holder->begin() + i);
		}
	}

	pthread_exit(nullptr);
	return 0;
}

#endif // !THREADING_H
