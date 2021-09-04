#ifndef HANDLE_H
#define HANDLE_H

#include "client.h"
#include "packet.h"

#include "generate_data.h"
#include "chunk_data.h"

#pragma region basic client packets

static void on_connect_handle(client* recv_client, packet* recv_packet)
{
	std::cout << "Client " << recv_client->client_id << "has authenticated its connection." << std::endl;
	recv_client->connection_state = 1;
}

static void launcher_data_handle(client* recv_client, packet* recv_packet)
{
	packet send_packet = packet(2);

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
	generate_data *temp_gd, generate_data* main_gd)
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
			emulate_chunk_gen(&send_packet, temp_gd, x, y);
		}
	}
	std::cout << "fuck" << std::endl;
	send_packet.send_packet(&recv_client->socket); // wtf is happening here with naming lolz
}

static void backend_save_handle(client * recv_client, packet * recv_packet,
	generate_data *main_gd, generate_data *temp_gd)
{
	if (!recv_client->is_backend)
	{
		std::cout << std::endl << "Non-backend client has called 'backend_save_handle'!" << std::endl;
		return;
	}

	if (recv_packet->read_bool())
	{
		generate_data new_gd = generate_data(recv_packet);
		main_gd = &new_gd;
	}
	else if (temp_gd->initialized)
	{
		(*main_gd) = temp_gd;
	}

	packet save_packet = packet(0, main_gd->estimate_serialized_size());
	main_gd->serialize(&save_packet);

	FILE* stream;
	int response = fopen_s(&stream, "generate_data.txt", "wb");

	if (response == 0)
	{
		fwrite(save_packet.buffer, 1, save_packet.readPos + 1, stream);
	}
	else { std::cout << std::endl << "Failed to write new generate data to file!" << std::endl; }
	if (stream) fclose(stream);
}

#pragma endregion

static void packet_handler(client* recv_client, packet* recv_packet, int id,
	generate_data* main_gd, generate_data* temp_gd)
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

	case 100: backend_login_handle(recv_client, recv_packet, main_gd);  break;
	case 101: backend_preview_handle(recv_client, recv_packet, temp_gd, main_gd); break;
	case 102: backend_save_handle(recv_client, recv_packet, main_gd, temp_gd); break;
	}

	recv_packet->dispose();
	//recv_packet = nullptr;
}

#endif