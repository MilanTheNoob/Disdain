#ifndef CHUNK_DATA_H
#define CHUNK_DATA_H

#include <cstdint>

#include "prop_gen.h"
#include "heightmap_gen.h"

#include "generate_data.h"
#include "vectors.h"

struct chunk_data
{
	int coord_x;
	int coord_y;

	int biome;

	int sample_center_x;
	int sample_center_y;

	uint16_t* heightmap;
	std::vector<prop_info> props;
	std::vector<player*> players;

	uint16_t color_r;
	uint16_t color_g;
	uint16_t color_b;

	chunk_data()
	{

	}

	chunk_data(packet* read_packet)
	{
		coord_x = read_packet->read_int32();
		coord_y = read_packet->read_int32();

		heightmap = new uint16_t[4096];
		for (int i = 0; i < 4096; i++) { heightmap[i] = read_packet->read_uint16(); }

		uint16_t prop_count = read_packet->read_uint16();
		for (int i = 0; i < prop_count; i++)
		{
			prop_info prop = prop_info();
			prop.group_id = read_packet->read_uint8();
			prop.prop_id = read_packet->read_uint8();

			prop.rotate_x = read_packet->read_int16();
			prop.rotate_y = read_packet->read_int16();
			prop.rotate_z = read_packet->read_int16();

			prop.pos_x = read_packet->read_int8();
			prop.pos_y = read_packet->read_int16();
			prop.pos_z = read_packet->read_int8();

			prop.scale = read_packet->read_uint8();
			props.push_back(prop);
		}
	}

	chunk_data(int seed, generate_data* gd, int x_coord, int y_coord,
		bool do_serialize = false, packet* send_packet = nullptr)
	{
		coord_x = x_coord;
		coord_y = y_coord;

		sample_center_x = x_coord * 64;
		sample_center_y = y_coord * 64;

		heightmap = generate_heightmap(gd, x_coord, y_coord, seed);
		props = generate_props(gd, seed, sample_center_x, sample_center_y, heightmap);

		if (do_serialize) serialize(send_packet);
	}

	void serialize(packet* send_packet)
	{
		if (send_packet == nullptr) return;
		for (int i = 0; i < 4096; i++) { send_packet->write_uint16(heightmap[i]); }

		send_packet->write_uint16(props.size());
		for (int i = 0; i < props.size(); i++)
		{
			send_packet->write_uint8(props[i].group_id);
			send_packet->write_uint8(props[i].prop_id);

			send_packet->write_int16(props[i].rotate_x);
			send_packet->write_int16(props[i].rotate_y);
			send_packet->write_int16(props[i].rotate_z);

			send_packet->write_int8(props[i].pos_x);
			send_packet->write_int16(props[i].pos_y);
			send_packet->write_int8(props[i].pos_z);

			send_packet->write_uint8(props[i].scale);
			// variant saving here
		}
	}

	int estimate_size() { return 4108 + (props.size() * 13); }

	void dispose()
	{
		delete[] heightmap;
	}
};

void emulate_chunk_gen(packet* send_packet, generate_data* gd, int x, int y)
{
	int sample_center_x = x * 64;
	int sample_center_y = y * 64;

	uint16_t* heightmap = generate_heightmap(gd, sample_center_x, sample_center_y, 0);
	for (int i = 0; i < 4096; i++) { send_packet->write_uint16(heightmap[i]); }

	std::vector<prop_info> props = generate_props(gd, 0, sample_center_x, sample_center_y, heightmap);
	delete heightmap;

	send_packet->write_uint16(props.size());
	for (int i = 0; i < props.size(); i++)
	{
		send_packet->write_uint8(props[i].group_id);
		send_packet->write_uint8(props[i].prop_id);

		send_packet->write_int16(props[i].rotate_x);
		send_packet->write_int16(props[i].rotate_y);
		send_packet->write_int16(props[i].rotate_z);

		send_packet->write_int8(props[i].pos_x);
		send_packet->write_int16(props[i].pos_y);
		send_packet->write_int8(props[i].pos_z);

		send_packet->write_uint8(props[i].scale);
		// variant saving here
	}
}

#endif