#ifndef LOBBY_DATA_H
#define LOBBY_DATA_H

#include <cstdint>
#include "chunk_data.h"

enum generate_type_enum 
{
	nothing,
	deserialize,
	generate,
};

struct lobby_data
{
	bool in_use = false;

	int lobby_size;
	int full_width;
	int chunk_count;

	chunk_data* chunks;

	lobby_data() {}
	lobby_data(int size)
	{
		lobby_size = size;
		full_width = size * 2;
		chunk_count = full_width * full_width;
	}

	void generate(generate_data* gd, int seed)
	{
		if (in_use)
		{
			for (int i = 0; i < chunk_count; i++) { delete[] chunks[i].heightmap; }
			delete[] chunks;
		}
		
		chunks = new chunk_data[chunk_count];
		int count = 0;

		for (int x = -lobby_size; x < lobby_size; x++)
		{
			for (int y = -lobby_size; y < lobby_size; y++)
			{
				chunks[count] = chunk_data(seed, gd, x, y);
				count++;
			}
		}
	}

	bool serialize(packet* serialize_packet)
	{
		if (!in_use) return false;

		int count = 0;
		serialize_packet->write_int32(lobby_size);

		for (int i = 0; i < chunk_count; i++)
		{
			chunks[count].serialize(serialize_packet);
		}
	}

	void deserialize(packet* data_packet)
	{
		if (in_use)
		{
			for (int i = 0; i < chunk_count; i++) { delete[] chunks[i].heightmap; }
			delete[] chunks;
		}

		int count = 0;
		in_use = true;

		lobby_size = data_packet->read_int32();
		chunk_count = (lobby_size * 2) * (lobby_size * 2);

		for (int i = 0; i < chunk_count; i++)
		{
			chunks[count] = chunk_data(data_packet);
		}
	}
};

#endif // !LOBBY_DATA_H
