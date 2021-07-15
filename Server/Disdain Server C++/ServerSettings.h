#include "Packet.h"
#include "Vectors.h"

#include <vector>

using namespace std;

#ifndef SERVERSETTINGS_H
#define SERVERSETTINGS_H

struct HeightmapData
{
	short scale = 50;
	short height_multiplier = 10;
	uint8_t octaves = 7;

	uint8_t persistence = 6; // Decimal number 1dp
	uint8_t lacunarity = 20; // Decimal number 2dp

	bool use_chance;
	// Range 0 to 255
	uint8_t chance;
};

struct PropData
{
	Vector3_16 rand_rotate;

	uint8_t max_scale; // Decimal number*
	uint8_t min_scale; // Decimal number*

	uint8_t generate_type;

	// Random GT
	uint8_t per_chunk_min;
	uint8_t per_chunk_max;

	// Random Chance GT
	uint8_t chance;
	// Perlin GT
	uint8_t perlin; // Decimal number to 2dp
	HeightmapData perlin_map;

	bool use_bounds;
	uint16_t bounds_x;
	uint16_t bounds_y;
	uint16_t bounds_z;
};

struct BiomeData
{
	vector<vector<PropData>> props;
	vector<HeightmapData> heightmaps;
};

struct GenerateData
{
	bool initialized;
	uint8_t chunk_size;

	vector<BiomeData> biomes;
	vector<HeightmapData> base_heightmap;

	HeightmapData biome_map;

	GenerateData(bool init) 
	{
		initialized = init;
		chunk_size = 64;

		HeightmapData h;
		h.scale = 50;
		h.height_multiplier = 10;
		h.octaves = 7;

		h.persistence = 6;
		h.lacunarity = 20;

		base_heightmap.push_back(h);
	}
	GenerateData(Packet packet)
	{
		initialized = true;

		for (int i = 0; i < packet.ReadChar(); i++)
		{
			BiomeData biome;

			for (int j = 0; j < packet.ReadChar(); j++)
			{
				vector<PropData> prop_group;

				for (int b = 0; b < packet.ReadChar(); b++)
				{
					PropData prop;

					prop.rand_rotate = Vector3_16(packet.ReadShort(), packet.ReadShort(), packet.ReadShort());

					prop.max_scale = packet.ReadChar();
					prop.min_scale = packet.ReadChar();

					prop.generate_type = packet.ReadChar();

					if (prop.generate_type == 0)
					{
						prop.per_chunk_min = packet.ReadChar();
						prop.per_chunk_max = packet.ReadChar();
					}
					else if (prop.generate_type == 1)
					{
						prop.chance = packet.ReadChar();
					}
					else if (prop.generate_type == 2)
					{
						prop.perlin = packet.ReadChar();
						prop.perlin_map = HeightmapData();

						prop.perlin_map.scale = packet.ReadShort();
						prop.perlin_map.height_multiplier = packet.ReadShort();
						prop.perlin_map.octaves = packet.ReadChar();

						prop.perlin_map.persistence = packet.ReadChar();
						prop.perlin_map.lacunarity = packet.ReadChar();
					}

					prop.use_bounds = packet.ReadBool();
					prop.bounds_x = packet.ReadShort();
					prop.bounds_y = packet.ReadShort();
					prop.bounds_z = packet.ReadShort();

					prop_group.push_back(prop);
				}

				biome.props.push_back(prop_group);
			}

			for (int j = 0; j < packet.ReadChar(); j++)
			{
				HeightmapData h;
				h.scale = packet.ReadShort();
				h.height_multiplier = packet.ReadShort();
				h.octaves = packet.ReadChar();

				h.persistence = packet.ReadChar();
				h.lacunarity = packet.ReadChar();

				h.use_chance = packet.ReadBool();
				h.chance = packet.ReadChar();

				biome.heightmaps.push_back(h);
			}
		}

		for (int j = 0; j < packet.ReadChar(); j++)
		{
			HeightmapData h;

			h.scale = packet.ReadShort();
			h.height_multiplier = packet.ReadShort();
			h.octaves = packet.ReadChar();

			h.persistence = packet.ReadChar();
			h.lacunarity = packet.ReadChar();

			base_heightmap.push_back(h);
		}

		biome_map.scale = packet.ReadShort();
		biome_map.height_multiplier = packet.ReadShort();
		biome_map.octaves = packet.ReadChar();

		biome_map.persistence = packet.ReadChar();
		biome_map.lacunarity = packet.ReadChar();

		biome_map.use_chance = packet.ReadBool();
		biome_map.chance = packet.ReadChar();
	}

	void Serialize(Packet packet)
	{
		packet.Write((uint8_t)biomes.size());
		for (uint8_t i = 0; i < biomes.size(); i++)
		{
			packet.Write((uint8_t)biomes[i].props.size());
			for (uint8_t j = 0; j < biomes[i].props.size(); j++)
			{
				packet.Write((uint8_t)biomes[i].props[j].size());
				for (uint8_t b = 0; b < packet.ReadChar(); b++)
				{
					packet.Write(biomes[i].props[j][b].rand_rotate.x);
					packet.Write(biomes[i].props[j][b].rand_rotate.y);
					packet.Write(biomes[i].props[j][b].rand_rotate.z);

					packet.Write(biomes[i].props[j][b].max_scale);
					packet.Write(biomes[i].props[j][b].min_scale);

					packet.Write(biomes[i].props[j][b].generate_type);
					if (biomes[i].props[j][b].generate_type == 0)
					{
						packet.Write(biomes[i].props[j][b].per_chunk_min);
						packet.Write(biomes[i].props[j][b].per_chunk_max);
					}
					else if (biomes[i].props[j][b].generate_type == 1)
					{
						packet.Write(biomes[i].props[j][b].chance);
					}
					else if (biomes[i].props[j][b].generate_type == 2)
					{
						packet.Write(biomes[i].props[j][b].perlin);

						packet.Write(biomes[i].props[j][b].perlin_map.scale);
						packet.Write(biomes[i].props[j][b].perlin_map.height_multiplier);
						packet.Write(biomes[i].props[j][b].perlin_map.octaves);

						packet.Write(biomes[i].props[j][b].perlin_map.persistence);
						packet.Write(biomes[i].props[j][b].perlin_map.lacunarity);
					}

					packet.Write(biomes[i].props[j][b].use_bounds);
					packet.Write(biomes[i].props[j][b].bounds_x);
					packet.Write(biomes[i].props[j][b].bounds_y);
					packet.Write(biomes[i].props[j][b].bounds_z);
				}
			}

			packet.Write((uint8_t)biomes[i].heightmaps.size());
			for (uint8_t j = 0; j < biomes[i].heightmaps.size(); j++)
			{
				packet.Write(biomes[i].heightmaps[j].scale);
				packet.Write(biomes[i].heightmaps[j].height_multiplier);
				packet.Write(biomes[i].heightmaps[j].octaves);

				packet.Write(biomes[i].heightmaps[j].persistence);
				packet.Write(biomes[i].heightmaps[j].lacunarity);

				packet.Write(biomes[i].heightmaps[j].use_chance);
				packet.Write(biomes[i].heightmaps[j].chance);
			}
		}

		packet.Write((uint8_t)base_heightmap.size());
		for (uint8_t j = 0; j < base_heightmap.size(); j++)
		{
			packet.Write(base_heightmap[j].scale);
			packet.Write(base_heightmap[j].height_multiplier);
			packet.Write(base_heightmap[j].octaves);

			packet.Write(base_heightmap[j].persistence);
			packet.Write(base_heightmap[j].lacunarity);
		}

		packet.Write(biome_map.scale);
		packet.Write(biome_map.height_multiplier);
		packet.Write(biome_map.octaves);

		packet.Write(biome_map.persistence);
		packet.Write(biome_map.lacunarity);

		packet.Write(biome_map.use_chance);
		packet.Write(biome_map.chance);
	}
};

#endif