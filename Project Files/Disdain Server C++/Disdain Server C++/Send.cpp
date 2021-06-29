#include <iostream>
#include <WS2tcpip.h>
#include "Packet.h"
#include "Client.cpp"
#pragma comment (lib, "ws2_32.lib")

#ifndef SEND_CPP
#define SEND_CPP

void onConnect_send(Client client)
{
	Packet packet = Packet(true, 1);
	packet.Write(client.client_id);
	send(client.socket, packet.buffer, packet.readPos, 0);
}

void botCheck_send(int id)
{
	Packet packet = Packet(true, 2);
}

#endif 
