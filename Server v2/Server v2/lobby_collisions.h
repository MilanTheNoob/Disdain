#ifndef LOBBY_COLLISIONS_H
#define LOBBY_COLLISIONS_H

#include "server_data.h"

void lobby_collisions(server_data* sdata)
{
	for (int i = 0; i < sdata->lobby_players.size(); i++)
	{
		player* p = sdata->lobby_players[i];
		if (p->updated)
		{
			p->rotation.y = p->rotation.y - p->mouse_y;
			p->rotation.x = p->rotation.x + p->mouse_x;

			p->current_pos.x = p->old_pos.x + ((p->old_pos.x + 1.0f) * p->move_x);
			p->current_pos.y = p->old_pos.y + ((p->old_pos.y + 1.0f) * p->move_y);

			p->velocity = p->velocity - 9.8f;
			p->current_pos.z = p->old_pos.z - p->velocity;

			int chunk = (round(sdata->lobby_players[i]->current_pos.x / 64) * 64) +
				round(sdata->lobby_players[i]->current_pos.y / 64);

			bool is_grounded = false;
			p->updated = false;

			for (int j = 0; j < sdata->lobby.chunks[chunk].props.size(); j++)
			{
				prop_info* prop = &sdata->lobby.chunks[chunk].props[i];
				if (std::fabs(p->collider.center.x - prop->collider.center.x) >
					(p->collider.half_width.x + prop->collider.half_width.x)) {
					p->current_pos.x = p->old_pos.x;
				}
				if (std::fabs(p->collider.center.z - prop->collider.center.z) >
					(p->collider.half_width.z + prop->collider.half_width.z)) {
					p->current_pos.z = p->old_pos.z;
				}

				if (std::fabs(p->collider.center.y - prop->collider.center.y) >
					(p->collider.half_width.y + prop->collider.half_width.y)) {
					is_grounded = true; p->current_pos.y = p->old_pos.x;
				}
			}

			// check for ground mesh here

			int updated_chunk_x = round(sdata->lobby_players[i]->current_pos.x / 64);
			int updated_chunk_y = round(sdata->lobby_players[i]->current_pos.y / 64);
			int updated_chunk = (updated_chunk_x * sdata->lobby.full_width) + (updated_chunk_y + sdata->lobby.chunk_count);

			if (updated_chunk != p->old_chunk || !p->chunk_initialized)
			{
				std::vector<player*>* old_chunk_players = &sdata->lobby.chunks[p->old_chunk].players;
				for (int i = 0; i < old_chunk_players->size(); i++)
				{
					if ((*old_chunk_players)[i] == p) old_chunk_players->erase(old_chunk_players->begin() + i);
				}

				sdata->lobby.chunks[updated_chunk].players.push_back(p);

				int estimate = 0;
				for (int x = -p->render_distance; x < p->render_distance; x++)
				{
					for (int y = -p->render_distance; y < -p->render_distance; y++)
					{
						estimate = estimate + (sdata->lobby.chunks[((x +
							updated_chunk_x) * sdata->lobby.full_width) + (y + updated_chunk_y)].props.size() * 13) +
							4106;
					}
				}
				packet send_packet = packet(14, estimate);

				int chunk_length = (p->render_distance * 2) * (p->render_distance * 2);
				std::vector<int> new_chunks;

				for (int x = -p->render_distance; x < p->render_distance; x++)
				{
					for (int y = -p->render_distance; y < -p->render_distance; y++)
					{
						send_packet.write_int32(x + updated_chunk_x);
						send_packet.write_int32(y + updated_chunk_y);

						int chunk = ((x + updated_chunk_x) * sdata->lobby.full_width) + (y + updated_chunk_y);
						new_chunks.push_back(chunk);

						if (!p->chunk_initialized)
						{
							send_packet.write_bool(true);
							sdata->lobby.chunks[chunk].serialize(&send_packet);
						}
						else
						{
							bool found_chunk_cache = false;
							for (int b = 0; b < chunk_length; b++)
							{
								if (sdata->lobby.chunks[p->rendered_chunks[b]].coord_x == (x + updated_chunk_x) &&
									sdata->lobby.chunks[p->rendered_chunks[b]].coord_y == (y + updated_chunk_y))
								{
									send_packet.write_bool(false);
								}
							}

							if (!found_chunk_cache)
							{
								send_packet.write_bool(true);
								sdata->lobby.chunks[chunk].serialize(&send_packet);
							}
						}
					}
				}

				p->rendered_chunks = new_chunks;

				send_packet.send_packet(p->socket);
				if (is_grounded) p->velocity = 2.0f;

				p->old_chunk = updated_chunk;
				p->chunk_initialized = true;
			}

			p->old_pos = p->current_pos;
		}
	}

	for (int i = 0; i < sdata->lobby_players.size(); i++)
	{
		player* p = sdata->lobby_players[i];

		int estimate_length = 0;
		int count = 0;
		for (int x = -p->render_distance; x < p->render_distance; x++)
		{
			for (int y = -p->render_distance; y < p->render_distance; y++)
			{
				estimate_length = estimate_length + sdata->lobby.chunks[p->rendered_chunks[count]].players.size();
				count++;
			}
		}

		packet send_packet = packet(15, (estimate_length * 24) + 4);
		send_packet.write_int32(estimate_length);

		count = 0;
		for (int x = -p->render_distance; x < p->render_distance; x++)
		{
			for (int y = -p->render_distance; y < p->render_distance; y++)
			{
				for (int j = 0; j < sdata->lobby.chunks[p->rendered_chunks[count]].players.size(); j++)
				{
					player* tp = sdata->lobby.chunks[p->rendered_chunks[count]].players[j];
					send_packet.write_int32(tp->player_id);

					send_packet.write_vector3(tp->current_pos);
					send_packet.write_vector2(tp->rotation);
				}
				count++;
			}
		}

		send_packet.send_packet(p->socket);
	}
}

#endif // !LOBBY_COLLISIONS_H
