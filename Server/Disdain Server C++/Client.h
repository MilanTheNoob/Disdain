#ifndef CLIENT_H
#define CLIENT_H

#include <WinSock2.h>
#include <vector>
#include <map>

#include "packet.h"

struct client
{
public:
	bool initialized = false;
	bool is_backend = false;

	int client_id = -1;
	int thread = -1;
	int thread_id = -1;

	SOCKET socket;
	std::vector<packet> recving_packets;

	int bot_check_id = -1;
	int connection_state = -1;

	client() { initialized = false; }
	client(int _client_id, SOCKET _socket)
	{
		initialized = true;
		is_backend = false;

		client_id = _client_id;
		socket = _socket;

		bot_check_id = -1;
		connection_state = 0;

		
	}
};

#endif