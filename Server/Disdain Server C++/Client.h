#include <WinSock2.h>

#ifndef CLIENT_H
#define CLIENT_H

struct Client
{
public:
	Client() { initialized = false; }
	Client(int _client_id, SOCKET _socket)
	{
		initialized = true;

		client_id = _client_id;
		socket = _socket;

		botCheck_id = -1;
		connection_state = 0;
	}

	bool initialized;
	int client_id;
	int thread;
	int thread_id;
	SOCKET socket;
	int botCheck_id = -1;
	int connection_state;
};

#endif