#ifndef SERVERSETTINGS_H
#define SERVERSETTINGS_H

#include "packet.h"
#include "vectors.h"

//#include "heightmap_gen.h"
#include <vector>

struct heightmap_data
{
	int scale = 50;
	int height_multiplier = 10;
	int octaves = 7;

	float persistence = 0.6f;
	float lacunarity = 2;
};

struct prop_data
{
	uint8_t group_id;
	uint8_t prop_id;

	float rotate_x;
	float rotate_y;
	float rotate_z;

	float max_scale;
	float min_scale;

	uint8_t generate_type;

	// Random GT
	uint8_t per_chunk_min;
	uint8_t per_chunk_max;

	// Random Chance GT
	float chance;
	// Perlin GT
	float perlin;
	heightmap_data perlin_map;

	float bounds_x;
	float bounds_y;
};

struct biome_data
{
	std::vector<prop_data> props;
	std::vector<heightmap_data> heightmaps;
};

struct generate_data
{
	bool initialized;

	std::vector<biome_data> biomes;
	heightmap_data biome_map;

	int estimate_serialized_size()
	{
		int return_size = 0;

		for (int i = 0; i < biomes.size(); i++)
		{
			return_size += biomes[i].heightmaps.size() * 17;

			for (int j = 0; j < biomes[i].props.size(); j++)
			{
				if (biomes[i].props[j].generate_type == 2) return_size += 22;
				return_size += 46;
			}
		}

		return return_size;
	}

	heightmap_data read_heightmap_data(packet* recv_packet)
	{
		heightmap_data h;
		h.scale = recv_packet->read_int32();
		h.height_multiplier = recv_packet->read_int32();
		h.octaves = recv_packet->read_int32();

		h.persistence = recv_packet->read_float();
		h.lacunarity = recv_packet->read_float();

		return h;
	}

	void write_heightmap_data(heightmap_data data, packet* recv_packet)
	{
		recv_packet->write_int32(data.scale);
		recv_packet->write_int32(data.height_multiplier);
		recv_packet->write_int32(data.octaves);

		recv_packet->write_float(data.persistence);
		recv_packet->write_float(data.lacunarity);
	}

	generate_data(bool init = false)
	{
		initialized = init;

		biome_data hilly_eg;
		heightmap_data hilly_heightmap;

		hilly_heightmap.height_multiplier = 5;
		hilly_heightmap.scale = 5;
		hilly_heightmap.octaves = 1;
		hilly_heightmap.lacunarity = 1.4f;
		hilly_heightmap.persistence = 1;

		hilly_eg.heightmaps.push_back(hilly_heightmap);
		biomes.push_back(hilly_eg);

		biome_data plains_eg;
		heightmap_data plains_heightmap;

		plains_heightmap.height_multiplier = 20;
		plains_heightmap.scale = 100;
		plains_heightmap.octaves = 1;
		plains_heightmap.lacunarity = 1.4f;
		plains_heightmap.persistence = 1;

		plains_eg.heightmaps.push_back(plains_heightmap);
		biomes.push_back(plains_eg);

		heightmap_data biome_heightmap;

		biome_heightmap.height_multiplier = 1;
		biome_heightmap.scale = 100;
		biome_heightmap.octaves = 1;
		biome_heightmap.lacunarity = 1.4f;
		biome_heightmap.persistence = 1;

		biome_map = biome_heightmap;
	}
	generate_data(packet* recv_packet)
	{
		initialized = true;

		uint8_t bl = recv_packet->read_uint8();
		for (uint8_t i = 0; i < bl; i++)
		{
			biome_data biome;

			uint8_t pl = recv_packet->read_uint8();
			for (uint8_t b = 0; b < pl; b++)
			{
				prop_data prop;

				prop.group_id = recv_packet->read_uint8();
				prop.prop_id = recv_packet->read_uint8();

				prop.rotate_x = recv_packet->read_float();
				prop.rotate_y = recv_packet->read_float();
				prop.rotate_z = recv_packet->read_float();

				prop.max_scale = recv_packet->read_float();
				prop.min_scale = recv_packet->read_float();

				prop.generate_type = recv_packet->read_uint8();

				if (prop.generate_type == 0)
				{
					prop.per_chunk_min = recv_packet->read_uint8();
					prop.per_chunk_max = recv_packet->read_uint8();
				}
				else if (prop.generate_type == 1)
				{
					prop.chance = recv_packet->read_uint8();
				}
				else if (prop.generate_type == 2)
				{
					prop.perlin = recv_packet->read_float();
					prop.perlin_map = read_heightmap_data(recv_packet);
				}

				prop.bounds_x = recv_packet->read_float();
				prop.bounds_y = recv_packet->read_float();

				biome.props.push_back(prop);
			}

			uint8_t bhl = recv_packet->read_uint8();
			for (uint8_t j = 0; j < bhl; j++)
				biome.heightmaps.push_back(read_heightmap_data(recv_packet));

			biomes.push_back(biome);
		}

		biome_map = read_heightmap_data(recv_packet);
	}

	void serialize(packet* recv_packet)
	{
		recv_packet->write_uint8((uint8_t)biomes.size());
		for (uint8_t i = 0; i < biomes.size(); i++)
		{
			recv_packet->write_uint8((uint8_t)biomes[i].props.size());
			for (uint8_t j = 0; j < biomes[i].props.size(); j++)
			{
				recv_packet->write_uint8(biomes[i].props[j].prop_id);
				recv_packet->write_uint8(biomes[i].props[j].group_id);

				recv_packet->write_float(biomes[i].props[j].rotate_x);
				recv_packet->write_float(biomes[i].props[j].rotate_y);
				recv_packet->write_float(biomes[i].props[j].rotate_z);

				recv_packet->write_float(biomes[i].props[j].max_scale);
				recv_packet->write_float(biomes[i].props[j].min_scale);

				recv_packet->write_uint8(biomes[i].props[j].generate_type);
				if (biomes[i].props[j].generate_type == 0)
				{
					recv_packet->write_uint8(biomes[i].props[j].per_chunk_min);
					recv_packet->write_uint8(biomes[i].props[j].per_chunk_max);
				}
				else if (biomes[i].props[j].generate_type == 1)
				{
					recv_packet->write_uint8(biomes[i].props[j].chance);
				}
				else if (biomes[i].props[j].generate_type == 2)
				{
					recv_packet->write_float(biomes[i].props[j].perlin);
					write_heightmap_data(biomes[i].props[j].perlin_map, recv_packet);
				}

				recv_packet->write_float(biomes[i].props[j].bounds_x);
				recv_packet->write_float(biomes[i].props[j].bounds_y);
			}

			recv_packet->write_uint8(biomes[i].heightmaps.size());
			for (uint8_t j = 0; j < biomes[i].heightmaps.size(); j++)
				write_heightmap_data(biomes[i].heightmaps[j], recv_packet);
		}

		write_heightmap_data(biome_map, recv_packet);
	}
};

#endif