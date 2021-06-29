#ifndef CLIENT_CPP 
#define CLIENT_CPP

struct Client
{
public:
	Client() {}
	Client(int _client_id, int _socket)
	{
		client_id = _client_id;
		socket = _socket;

		botCheck_id = -1;
		connection_state = 0;
	}

	int client_id;
	int socket;
	int botCheck_id;
	int connection_state;
};

#endif