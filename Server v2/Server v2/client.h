#ifndef CLIENT_H
#define CLIENT_H

#include <WinSock2.h>
#include <vector>
#include <map>

#include "packet.h"
#include "account_data.h"
#include "collision.h"
#include "player.h"

enum class bot_check_enum
{
	uninitialized = 0,
	called = 1,
	failed = 2,
	completed = 3,
};
enum class sign_up_enum
{
	unused,
	bot_check,
	email_send,
	email_recieve,
	username,
	password,
	character_setup,
	s_completed,
};
enum class log_in_enum
{
	l_unused,
	l_bot_check,
	l_email_send,
	l_password,
	l_email_recieve,
	l_completed,
};
enum class connection_state_enum
{
	unidentified,
	lobby,
	save,
	server,
};

struct client
{
	bool is_backend;

	int client_id;
	int thread;
	int thread_id;

	SOCKET socket;
	std::vector<packet> recving_packets;
	collision_object collider;

	connection_state_enum connection_state = connection_state_enum::unidentified;
	sign_up_enum signup_state = sign_up_enum::unused;
	log_in_enum login_state = log_in_enum::l_unused;
	bot_check_enum bot_check = bot_check_enum::uninitialized;

	account_data* account;
	account_data* temp_account;
	int account_pos;

	int email_code;
	uint8_t bot_check_attempts;
	int bot_check_value;

	bool player_setup;
	player player_object;

	client() {}
	client(int _client_id, SOCKET _socket)
	{
		is_backend = false;

		client_id = _client_id;
		socket = _socket;
	}

	void setup_player()
	{
		player_object = player();
		player_object.player_id = client_id;

		player_object.initialized = true;
		player_object.chunk_initialized = false;

		player_object.render_distance = 5;
		player_object.updated = true;

		player_object.collider = collision_object();
		player_object.collider.half_width = vector3(0.2f, 1.9f, 0.15f);
		player_object.collider.center = vector3(0.0f, 0.95f, 0.0f);
	}
};

#endif