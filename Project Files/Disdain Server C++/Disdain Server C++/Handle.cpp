#ifndef HANDLE_CPP
#define HANDLE_CPP

#include "Client.cpp"

#include <iostream>
#include "Packet.h"

void onConnect_handle(Client c, Packet packet)
{
	std::cout << "yo!" << std::endl;
	//c.client_state == ClientStateEnum::ConnectedIdle;
}

#endif
