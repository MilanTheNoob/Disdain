#ifndef HEIGHTMAP_GEN_H
#define HEIGHTMAP_GEN_H

#include "generate_data.h"
#include "simplex_noise.h"
#include "weighting.h"
#include "math.h"

#include <cstdlib>

uint16_t* generate_heightmap(generate_data* gd, int sample_center_x, int sample_center_y, int seed)
{
	uint16_t* return_heightmap = new uint16_t[4096];
	int n = gd->biomes.size();

	sample_center_x = sample_center_x - 32;
	sample_center_y = sample_center_y - 32;

	int count = 0;

	for (int x = 0; x < 64; x++)
	{
		for (int y = 0; y < 64; y++)
		{
			float l = (noise(&gd->biome_map, seed, sample_center_x + x, sample_center_y + y) + 1) / (float)2;
			float noise_height = 0;

			uint8_t biomes_count = 0;
			uint8_t id = 0;

			for (int i = 0; i < n; i++)
			{
				if (use_biome(n, l, i))
				{
					biomes_count++;
					id = i;
				}
			}

			if (biomes_count < 2)
			{
				for (int j = 0; j < gd->biomes[id].heightmaps.size(); j++)
				{
					noise_height = noise_height + noise(&gd->biomes[id].heightmaps[j],
						seed + j, sample_center_x + x, sample_center_y + y);
				}
			}
			else
			{
				for (int i = 0; i < n; i++)
				{
					if (use_biome(n, l, i))
					{
						float temp_noise = 0;

						for (int j = 0; j < gd->biomes[i].heightmaps.size(); j++)
						{
							temp_noise = temp_noise + noise(&gd->biomes[i].heightmaps[j],
								seed + j, sample_center_x + x, sample_center_y + y);
						}
						noise_height = noise_height + (temp_noise * biome_weight(n, l, i));
					}
				}
			}

			return_heightmap[count] = (uint16_t)(noise_height * 10);
			count++;
		}
	}

	return return_heightmap;
}

#endif