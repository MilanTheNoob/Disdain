#include <iostream>
#include <WS2tcpip.h>
#include <string>
#include <sstream>
#include <map>

#include "Client.cpp"

#include "Handle.cpp"
#include "Send.cpp"
//#include "Packet.h"

#pragma comment (lib, "ws2_32.lib")

void main()
{
	std::map<int, Client> clients;

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
	FD_ZERO(&master);
	FD_SET(listening, &master);

	bool running = true;
	while (running)
	{
		fd_set copy = master;
		int socketCount = select(0, &copy, nullptr, nullptr, nullptr);

		for (int i = 0; i < socketCount; i++)
		{
			SOCKET sock = copy.fd_array[i];

			if (sock == listening)
			{
				SOCKET socket = accept(listening, nullptr, nullptr);
				FD_SET(socket, &master);

				Client client = Client(clients.size(), socket);
				clients.insert(std::make_pair(socket, client));

				Packet packet = Packet(true, 1);
				packet.Write(client.client_id);

				send(socket, packet.buffer, sizeof(packet.buffer), 0);
			}
			else
			{
				Packet packet = Packet(false, 0);

				int bytesIn = recv(sock, packet.buffer, 4096, 0);
				if (bytesIn <= 0)
				{
					clients.erase(sock);

					closesocket(sock);
					FD_CLR(sock, &master);
				}
				else
				{
					switch (packet.ReadInt())
					{
					case 1: onConnect_handle(clients[sock], packet); break; // OnConnect
					//case 2:
					}
				}
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