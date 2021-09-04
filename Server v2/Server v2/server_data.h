#ifndef SERVER_DATA_H
#define SERVER_DATA_H

#include <pthread.h>
#include <map>

#include "generate_data.h"
#include "client.h"
#include "lobby_data.h"

struct server_data
{
	std::map<SOCKET, client*> clients;
	lobby_data lobby;

	generate_data main_gd = generate_data(true);
	generate_data temp_gd = generate_data(false);

	std::vector<account_data> accounts;

	std::vector<player*> lobby_players;
	std::vector<player*> save_players;
};

#endif // !SERVER_DATA_H
