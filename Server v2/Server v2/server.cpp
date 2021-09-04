#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>
#include <pthread.h>

#include "client.h"
#include "account_data.h"

#include "packet.h"
#include "generate_data.h"
#include "server_data.h"

#include "file.h"
#include "math.h"
#include "send.h"

#include "socket_thread.h"
#include "game_thread.h"

#pragma comment (lib, "ws2_32.lib")
server_data data;

pthread_t gthread;
game_thread_data game_data;

std::vector<pthread_t*> threads;
std::vector<thread_data> thread_info;

bool server_running = true;

void main()
{
	std::cout << "hello world" << std::endl << "loading hard data..." << std::endl << std::endl;

	srand(time(NULL));

	file_data* gdh = read_file("generate_data.txt");
	if (gdh->success)
	{
		data.main_gd = generate_data(&gdh->serialize_packet);
		gdh->serialize_packet.dispose_packet();

		std::cout << "successfully loaded generate data" << std::endl;
	}
	else { std::cout << "unsuccessfully loaded generate data" << std::endl; }
	delete gdh;

	file_data* ldh = read_file("lobby_data.txt");
	if (ldh->success)
	{
		data.lobby = lobby_data();
		data.lobby.deserialize(&ldh->serialize_packet);

		ldh->serialize_packet.dispose_packet();
		std::cout << "successfully loaded lobby data" << std::endl;
	}
	else
	{
		data.lobby = lobby_data(10);
		data.lobby.generate(&data.main_gd, 0);

		std::cout << "unsuccessfully loaded lobby data" << std::endl;
	}
	delete ldh;

	file_data* ph = read_file("player_data.txt");
	if (ph->success)
	{
		int length = ph->serialize_packet.read_int32();

		for (int i = 0; i < length; i++)
		{
			account_data account = account_data();

			account.username = ph->serialize_packet.read_string();
			account.password = ph->serialize_packet.read_string();
			account.email = ph->serialize_packet.read_string();

			account.skin = skin_data();
			account.skin.gender = ph->serialize_packet.read_bool();
			account.skin.skin = ph->serialize_packet.read_int32();

			account.skin.head = ph->serialize_packet.read_int32();
			account.skin.hair = ph->serialize_packet.read_int32();

			account.skin.hat = ph->serialize_packet.read_int32();
			account.skin.shirt = ph->serialize_packet.read_int32();
			account.skin.pants = ph->serialize_packet.read_int32();
			account.skin.shoes = ph->serialize_packet.read_int32();
			account.skin.accessory = ph->serialize_packet.read_int32();

			account.skin.fatness = ph->serialize_packet.read_float();
			account.skin.muscles = ph->serialize_packet.read_float();
			account.skin.slimness = ph->serialize_packet.read_float();
			account.skin.thinness = ph->serialize_packet.read_float();
			account.skin.breasts = ph->serialize_packet.read_float();

			data.accounts.push_back(account);
		}

		ph->serialize_packet.dispose_packet();
		std::cout << "successfully loaded player data" << std::endl;
	} else { std::cout << "unsuccessfully loaded player data" << std::endl; }
	delete ph;

	std::cout << std::endl << "starting game thread" << std::endl;

	game_data = game_thread_data();
	game_data.running = &server_running;
	game_data.sdata = &data;

	int response = pthread_create(&gthread, nullptr, game_thread, (void*)&game_data);

	std::cout << "starting winsock" << std::endl;

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

	std::cout << "server fully started up" << std::endl << "have a great day!" << std::endl;

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

			client new_client = client(data.clients.size(), socket);
			data.clients.insert(std::make_pair(socket, &new_client));

			on_connect_send(data.clients.size(), &socket);
			std::cout << "Client " << new_client.client_id << " has been connected. Adding to a thread..." << std::endl;

			bool inThread = false;
			int thread_loop_count = 0;

			while (!inThread && thread_loop_count < threads.size())
			{
				if (thread_info[thread_loop_count].player_count + thread_info[thread_loop_count].incoming_clients.size() < 64)
				{
					std::cout << "Client " << new_client.client_id << " has been added to thread " << thread_loop_count << std::endl;

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
				thread_info[new_thread_id].sdata = &data;

				thread_info[new_thread_id].threads = &threads;
				thread_info[new_thread_id].thread_info = &thread_info;

				pthread_t thr;
				int response = pthread_create(&thr, nullptr, socket_group_thread, (void*)&thread_info[new_thread_id]);
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

	packet serialize_packet = packet(0, (data.accounts.size() * 250) + 4);
	serialize_packet.write_int32(data.accounts.size());

	for (int i = 0; i < data.accounts.size(); i++)
	{
		serialize_packet.write_string(data.accounts[i].username);
		serialize_packet.write_string(data.accounts[i].password);
		serialize_packet.write_string(data.accounts[i].email);

		serialize_packet.write_bool(data.accounts[i].skin.gender);
		serialize_packet.write_int32(data.accounts[i].skin.skin);
		
		serialize_packet.write_int32(data.accounts[i].skin.head);
		serialize_packet.write_int32(data.accounts[i].skin.hair);

		serialize_packet.write_int32(data.accounts[i].skin.hat);
		serialize_packet.write_int32(data.accounts[i].skin.shirt);
		serialize_packet.write_int32(data.accounts[i].skin.pants);
		serialize_packet.write_int32(data.accounts[i].skin.shoes);
		serialize_packet.write_int32(data.accounts[i].skin.accessory);

		serialize_packet.write_float(data.accounts[i].skin.fatness);
		serialize_packet.write_float(data.accounts[i].skin.muscles);
		serialize_packet.write_float(data.accounts[i].skin.slimness);
		serialize_packet.write_float(data.accounts[i].skin.thinness);
		serialize_packet.write_float(data.accounts[i].skin.breasts);
	}

	write_file("player_data.txt", &serialize_packet);
	serialize_packet.dispose_packet();

	WSACleanup();
	system("pause");
}