#ifndef HANDLE_H
#define HANDLE_H

#include "client.h"
#include "packet.h"

#include "generate_data.h"
#include "chunk_data.h"
#include "file.h"

/*

listed below are client-to-server packet ids:
	0. on connect
	1. launcher data
	2. quick login

	3. bot check start (signup)
	4. bot check attempt (signup)

	5. email send (signup)
	6. email receive (signup)
	
	7. username handle (signup)
	8. password handle (signup)
	9. skin submit (signup)
	
	10. bot check start (login)
	11. bot check attempt (login)

	12. email send (login)
	13. password receive (login)
	14. email receive (login)

	15. enter lobby
	16. exit lobby

	17. lobby movement

	100. backend login
	101. backend preview
	102. backend save

listed below are server-to-client packet ids:
	0. on connect
	1. launcher data
	2. quick login response

	3. bot check send (signup)
	4. bot check attempt response (signup)

	5. email receieve response (signup)
	6. username response (signup)
	7. unused

	8. bot check send (login)
	9. bot check attempt response (login)

	10. find account by email (login)
	11. enter account by password (login)
	12. email receive response (login)

	13. enter lobby response
	14. exit lobby response

	15. lobby render data
	16. lobby positional data
*/

#pragma region basic client packets

static void on_connect_handle(client* recv_client, packet* recv_packet)
{
	std::cout << "Client " << recv_client->client_id << " has authenticated its connection." << std::endl;
	//recv_client->connection_state = 1;
}

static void launcher_data_handle(client* recv_client, packet* recv_packet)
{
	packet send_packet = packet(1);

	bool update = false;
	if (recv_packet->read_int32() != 1) update = true;
	if (recv_packet->read_int32() != 1) update = true;
	if (recv_packet->read_int32() != 1) update = true;
	send_packet.write_bool(update);

	if (update)
	{
		send_packet.write_uint8(1);
		send_packet.write_uint8(1);
		send_packet.write_uint8(1);
	}

	send_packet.write_uint8(0); // Todo : Add patch notes & news here
	send_packet.send_packet(&recv_client->socket);
}

static void quick_login_handle(client* recv_client, packet* recv_packet, std::vector<account_data>* accounts)
{
	std::string username = recv_packet->read_string();
	std::string password = recv_packet->read_string();

	bool found_account = false;
	for (int i = 0; i < accounts->size(); i++)
	{
		if ((*accounts)[i].username == username && (*accounts)[i].password == password)
		{
			found_account = true;
			packet send_packet = packet(2);

			bool yes = true;
			send_packet.write_bool(yes);

			send_packet.send_packet(&recv_client->socket, true);
		}
	}

	if (!found_account)
	{
		packet send_packet = packet(2);

		bool no = false;
		send_packet.write_bool(no);

		send_packet.send_packet(&recv_client->socket, true);
	}
}

#pragma endregion
#pragma region signup packets

static void bot_check_handle(client* recv_client, packet* recv_packet)
{
	const int checks[] = { 6285, 4921, 7301, 3054 };
	const int size = 3;

	if (recv_client->bot_check == bot_check_enum::uninitialized)
	{
		recv_client->signup_state = sign_up_enum::bot_check;
		random_local rnd = random_local(rand());

		FILE* stream;
		int rand_value = rnd.range(0, size);
		std::string file_name = std::to_string(checks[rand_value]) + ".png";

		std::cout << "captcha " << rand_value << std::endl;
		int response = fopen_s(&stream, file_name.c_str(), "rb");

		fseek(stream, 0, SEEK_END);
		int stream_size = ftell(stream);
		rewind(stream);

		std::cout << stream_size << std::endl;

		packet send_packet = packet(3, stream_size + 5);
		uint8_t yes = 1;
		send_packet.write_uint8(yes);
		send_packet.write_int32(stream_size);

		response = fread(&send_packet.buffer[5], 1, stream_size, stream);
		fclose(stream);

		send_packet.writePos = send_packet.writePos + stream_size;
		send_packet.send_packet(&recv_client->socket);

		recv_client->bot_check = bot_check_enum::called;
		recv_client->bot_check_attempts = 3;
		recv_client->bot_check_value = checks[rand_value];
	}
	else
	{
		packet send_packet = packet(3);
		uint8_t no = 0;
		send_packet.write_uint8(no);

		send_packet.send_packet(&recv_client->socket);
	}
}

static void bot_check_submit_handle(client* recv_client, packet* recv_packet)
{
	if (recv_client->signup_state == sign_up_enum::bot_check)
	{
		recv_client->bot_check_attempts = recv_client->bot_check_attempts - 1;
		packet send_packet = packet(4);

		if (recv_client->bot_check == bot_check_enum::called && recv_client->bot_check_attempts > 0
			&& recv_client->bot_check_value == recv_packet->read_int32())
		{
			send_packet.write_bool(true);

			recv_client->signup_state = sign_up_enum::email_send;
			recv_client->bot_check = bot_check_enum::completed;
		}
		else
		{
			send_packet.write_bool(false);
			send_packet.write_uint8(recv_client->bot_check_attempts);
		}

		send_packet.send_packet(&recv_client->socket);
	}
}

static void email_send_handle(client* recv_client, packet* recv_packet, std::vector<account_data>* accounts)
{
	if (recv_client->signup_state == sign_up_enum::email_send)
	{
		accounts->push_back(account_data());

		recv_client->account_pos = accounts->size() - 1;
		recv_client->account = &(*accounts)[recv_client->account_pos];

		recv_client->account->email = recv_packet->read_string();

		random_local rnd = random_local(rand());
		recv_client->email_code = rnd.range(0, 9999);

		std::cout << recv_client->email_code << " < email code" << std::endl;
		recv_client->signup_state = sign_up_enum::email_recieve;

		// Send email here
	}
}

static void email_recieve_handle(client* recv_client, packet* recv_packet)
{
	if (recv_client->signup_state == sign_up_enum::email_recieve)
	{
		packet send_packet = packet(5);

		if (recv_packet->read_int32() == recv_client->email_code)
		{
			recv_client->signup_state = sign_up_enum::username;
			send_packet.write_bool(true);
		}
		else
		{
			send_packet.write_bool(false);
		}

		send_packet.send_packet(&recv_client->socket);
	}
}

static void username_handle(client* recv_client, packet* recv_packet, std::vector<account_data>* accounts)
{
	if (recv_client->signup_state == sign_up_enum::username)
	{
		packet send_packet = packet(6);

		std::string username = recv_packet->read_string();
		bool found_username = true;

		for (int i = 0; i < accounts->size(); i++)
		{
			if ((*accounts)[i].username == username)
			{
				found_username = false;
				break;
			}
		}
		
		send_packet.write_bool(found_username);
		if (found_username)
		{
			recv_client->account->username = username;
			recv_client->signup_state = sign_up_enum::password;
		}
		else
		{
			recv_packet->write_string("Username already done been taken");
		}
		send_packet.send_packet(&recv_client->socket);
	}
}

static void password_handle(client* recv_client, packet* recv_packet)
{
	if (recv_client->signup_state == sign_up_enum::password)
	{
		recv_client->account->password = recv_packet->read_string();
		recv_client->signup_state = sign_up_enum::character_setup;
	}
}

static void skin_handle(client* recv_client, packet* recv_packet)
{
	if (recv_client->signup_state == sign_up_enum::character_setup)
	{
		recv_client->signup_state = sign_up_enum::s_completed;
		skin_data skin = skin_data();

		skin.gender = recv_packet->read_bool();
		skin.skin = recv_packet->read_int32();

		skin.head = recv_packet->read_int32();
		skin.hair = recv_packet->read_int32();

		skin.hat = recv_packet->read_int32();
		skin.shirt = recv_packet->read_int32();
		skin.pants = recv_packet->read_int32();
		skin.shoes = recv_packet->read_int32();
		skin.accessory = recv_packet->read_int32();
		
		skin.fatness = recv_packet->read_float();
		skin.muscles = recv_packet->read_float();
		skin.slimness = recv_packet->read_float();
		skin.thinness = recv_packet->read_float();
		skin.breasts = recv_packet->read_float();

		recv_client->account->skin = skin;
		recv_client->account->fully_initialized = true;
	}
}

#pragma endregion
#pragma region login packets

static void bot_check_login(client* recv_client, packet* recv_packet)
{
	const int checks[] = { 6285, 4921, 7301, 3054, 0213 };
	const int size = 4;

	if (recv_client->bot_check == bot_check_enum::uninitialized)
	{
		recv_client->login_state = log_in_enum::l_bot_check;
		random_local rnd = random_local(rand());

		FILE* stream;
		int rand_value = rnd.range(0, size);
		std::string file_name = std::to_string(checks[rand_value]) + ".png";

		std::cout << "captcha " << rand_value  << std::endl;
		int response = fopen_s(&stream, file_name.c_str(), "rb");

		fseek(stream, 0, SEEK_END);
		int stream_size = ftell(stream);
		rewind(stream);

		std::cout << stream_size << std::endl;

		packet send_packet = packet(8, stream_size + 5);
		send_packet.write_bool(true);
		send_packet.write_int32(stream_size);

		response = fread(&send_packet.buffer[5], 1, stream_size, stream);
		fclose(stream);

		send_packet.writePos = send_packet.writePos + stream_size;
		send_packet.send_packet(&recv_client->socket);

		recv_client->bot_check = bot_check_enum::called;
		recv_client->bot_check_attempts = 3;
		recv_client->bot_check_value = checks[rand_value];
	}
	else
	{
		packet send_packet = packet(8);
		send_packet.write_bool(false);

		send_packet.send_packet(&recv_client->socket);
	}
}

static void bot_check_response_login(client* recv_client, packet* recv_packet)
{
	if (recv_client->login_state == log_in_enum::l_bot_check)
	{
		recv_client->bot_check_attempts = recv_client->bot_check_attempts - 1;
		packet send_packet = packet(9);

		if (recv_client->bot_check == bot_check_enum::called && recv_client->bot_check_attempts > 0
			&& recv_client->bot_check_value == recv_packet->read_int32())
		{
			send_packet.write_bool(true);

			recv_client->login_state = log_in_enum::l_email_send;
			recv_client->bot_check = bot_check_enum::completed;
		}
		else
		{
			send_packet.write_bool(false);
			send_packet.write_uint8(recv_client->bot_check_attempts);
		}

		send_packet.send_packet(&recv_client->socket);
	}
}

static void email_send_login(client* recv_client, packet* recv_packet, std::vector<account_data>* accounts)
{
	if (recv_client->login_state == log_in_enum::l_email_send)
	{
		packet send_packet = packet(10);

		bool found_account = false;
		for (int i = 0; i < accounts->size(); i++)
		{
			if ((*accounts)[i].email == recv_packet->read_string())
			{
				found_account = true;
				recv_client->temp_account = &(*accounts)[i];

				send_packet.write_bool(true);
				recv_client->login_state = log_in_enum::l_password;
			}
		}

		if (!found_account) send_packet.write_bool(false);
		send_packet.send_packet(&recv_client->socket);
	}
}

static void password_login(client* recv_client, packet* recv_packet)
{
	if (recv_client->login_state == log_in_enum::l_password)
	{
		packet send_packet = packet(11);
		if (recv_client->temp_account->password == recv_packet->read_string())
		{
			send_packet.write_bool(true);
			recv_client->login_state = log_in_enum::l_email_recieve;

			random_local rnd = random_local(rand());
			recv_client->email_code = rnd.range(0, 9999);

			std::cout << recv_client->email_code << " < email code" << std::endl;
		}
		else { send_packet.write_bool(false); }

		send_packet.send_packet(&recv_client->socket);
	}
}

static void email_attempt_login(client* recv_client, packet* recv_packet)
{
	if (recv_client->login_state == log_in_enum::l_email_recieve)
	{
		packet send_packet = packet(12);
		if (recv_client->email_code == recv_packet->read_int32())
		{
			send_packet.write_bool(true);

			recv_client->account = recv_client->temp_account;
			recv_client->login_state = log_in_enum::l_completed;
		}
		else { send_packet.write_bool(false); }

		send_packet.send_packet(&recv_client->socket);
	}
}

#pragma endregion

#pragma region basic lobby packets

static void enter_lobby(client* recv_client, packet* recv_packet, server_data* sdata)
{
	std::cout << "enter!" << std::endl;
	packet send_packet = packet(13);
	send_packet.write_bool(true);
	send_packet.write_vector3(vector3(0.0f, 20.0f, 0.0f));
	send_packet.send_packet(&recv_client->socket);

	if (!recv_client->player_setup) recv_client->setup_player();
	switch (recv_client->connection_state)
	{
	case connection_state_enum::save:
		for (int i = 0; i < sdata->save_players.size(); i++) {
			if (sdata->save_players[i] == &recv_client->player_object)
			{
				sdata->save_players.erase(sdata->save_players.begin() + i);
			}
		} break;
	case connection_state_enum::server:
		// unused
		break;
	}

	sdata->lobby_players.push_back(&recv_client->player_object);
	recv_client->connection_state = connection_state_enum::lobby;
}

static void exit_lobby(client* recv_client, packet* recv_packet, server_data* sdata)
{
	packet send_packet = packet(14);
	if (recv_client->connection_state == connection_state_enum::lobby)
	{
		send_packet.write_bool(true);
		for (int i = 0; i < sdata->lobby_players.size(); i++)
		{
			if (sdata->lobby_players[i] == &recv_client->player_object)
				sdata->lobby_players.erase(sdata->lobby_players.begin() + i);
		}

		recv_client->player_object.current_pos = vector3(0.0f, 0.0f, 0.0f);
		recv_client->player_object.old_pos = vector3(0.0f, 0.0f, 0.0f);

		recv_client->player_object.rotation = vector2(0.0f, 0.0f);
		recv_client->player_object.velocity = 0.0f;
	}
}

static void lobby_movement(client* recv_client, packet* recv_packet)
{
	if (recv_client->connection_state == connection_state_enum::lobby && !recv_client->player_object.updated)
	{
		recv_client->player_object.move_x = recv_packet->read_float();
		recv_client->player_object.move_y = recv_packet->read_float();

		recv_client->player_object.mouse_x = recv_packet->read_float();
		recv_client->player_object.mouse_y = recv_packet->read_float();

		recv_client->player_object.updated = true;
	}
}

#pragma endregion

#pragma region backend client packets

static void backend_login_handle(client* recv_client, packet* recv_packet, generate_data* main_gd)
{
	if (recv_packet->read_string() == "11ac1c39-1957-44c0-aa3e-2ed005581592")
	{
		recv_client->is_backend = true;
		std::cout << "Client " << recv_client->client_id << " is now a backend client" << std::endl;

		packet send_packet = packet(100, main_gd->estimate_serialized_size() + 4);

		int is_true = 69;
		send_packet.write_int32(is_true);

		main_gd->serialize(&send_packet);
		send_packet.send_packet(&recv_client->socket);
	}
	else
	{
		std::cout << "Client " << recv_client->client_id <<
			" has tried and failed to login as a backend client. Kicking" << std::endl;

		// UN IMPLEMENTED
	}
}

static void backend_preview_handle(client* recv_client, packet* recv_packet,
	generate_data* temp_gd, generate_data* main_gd)
{
	if (!recv_client->is_backend)
	{
		std::cout << "\nNon-backend client has called 'backend_preview_handle'!" << std::endl;
		return;
	}

	int size = recv_packet->read_int32();
	generate_data new_gd = generate_data(recv_packet);

	//delete temp_gd;
	temp_gd = &new_gd;

	if (size < 1) size = 1;
	if (size > 100) size = 100;

	std::cout << "\nBackend client requesting preview..." << std::endl
		<< "Sending " << unsigned(size * size) << " chunks to backend client for previewing" << std::endl;

	packet send_packet = packet(101, size * size * 8192);
	send_packet.write_uint8(size);

	for (uint8_t x = 0; x < size; x++)
	{
		for (uint8_t y = 0; y < size; y++)
		{
			emulate_chunk_gen(&send_packet, &new_gd, x, y);
		}
	}
	send_packet.send_packet(&recv_client->socket); // wtf is happening here with naming lolz
}

static void backend_save_handle(client* recv_client, packet* recv_packet,
	generate_data* main_gd, generate_data* temp_gd)
{
	if (!recv_client->is_backend)
	{
		std::cout << std::endl << "non-backend client has called 'backend_save_handle'!" << std::endl;
		return;
	}

	std::cout << std::endl << "backend_save_handle has been called" << std::endl;
	
	generate_data data = generate_data(recv_packet);

	temp_gd = &data;
	main_gd = &data;

	packet save_packet = packet(0, main_gd->estimate_serialized_size());
	data.serialize(&save_packet);

	write_file("generate_data.txt", &save_packet);
}

#pragma endregion

static void packet_handler(client* recv_client, packet* recv_packet, int id, server_data* sdata)
{
	if (!recv_packet->singular)
	{
		for (int i = 0; i < recv_client->recving_packets.size(); i++)
		{
			if (recv_client->recving_packets[i].packet_id == id)
				recv_client->recving_packets.erase(recv_client->recving_packets.begin() + i);
		}
	}

	switch (id)
	{
	case 0: on_connect_handle(recv_client, recv_packet); break;
	case 1: launcher_data_handle(recv_client, recv_packet); break;
	case 2: quick_login_handle(recv_client, recv_packet, &sdata->accounts); break;

	case 3: bot_check_handle(recv_client, recv_packet); break;
	case 4: bot_check_submit_handle(recv_client, recv_packet); break;

	case 5: email_send_handle(recv_client, recv_packet, &sdata->accounts); break;
	case 6: email_recieve_handle(recv_client, recv_packet); break;

	case 7: username_handle(recv_client, recv_packet, &sdata->accounts); break;
	case 8: password_handle(recv_client, recv_packet); break;
	case 9: skin_handle(recv_client, recv_packet); break;

	case 10: bot_check_login(recv_client, recv_packet); break;
	case 11: bot_check_response_login(recv_client, recv_packet); break;
	case 12: email_send_login(recv_client, recv_packet, &sdata->accounts); break;
	case 13: password_login(recv_client, recv_packet); break;
	case 14: email_attempt_login(recv_client, recv_packet); break;

	case 15: enter_lobby(recv_client, recv_packet, sdata); break;
	case 16: exit_lobby(recv_client, recv_packet, sdata); break;

	case 17: lobby_movement(recv_client, recv_packet); break;

	case 100: backend_login_handle(recv_client, recv_packet, &sdata->main_gd);  break;
	case 101: backend_preview_handle(recv_client, recv_packet, &sdata->temp_gd, &sdata->main_gd); break;
	case 102: backend_save_handle(recv_client, recv_packet, &sdata->main_gd, &sdata->temp_gd); break;
	}

	recv_packet->dispose_packet();
	//recv_packet = nullptr;
}

#endif