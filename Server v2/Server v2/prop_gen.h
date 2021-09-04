#ifndef PROP_GEN_H
#define PROP_GEN_H

#include "generate_data.h"
#include "heightmap_gen.h"
#include "weighting.h"

#include <vector>
#include "random.h"

struct prop_info
{
	uint8_t group_id;
	uint8_t prop_id;

	int16_t rotate_x;
	int16_t rotate_y;
	int16_t rotate_z;

	int8_t pos_x;
	int16_t pos_y;
	int8_t pos_z;

	uint8_t scale;
	uint8_t variant;

	collision_object collider;
};

prop_info add_prop(prop_data* data, random_local* rnd, int x_pos, int y_pos, int z_pos)
{
	prop_info prop;

	prop.group_id = data->group_id;
	prop.prop_id = data->prop_id;

	prop.pos_x = x_pos;
	prop.pos_y = y_pos;
	prop.pos_z = z_pos;

	prop.rotate_x = rnd->range(-data->rotate_x, data->rotate_x);
	prop.rotate_y = rnd->range(-data->rotate_y, data->rotate_y);
	prop.rotate_z = rnd->range(-data->rotate_z, data->rotate_z);

	prop.scale = rnd->range(data->min_scale * 10, data->max_scale * 10);

	return prop;
}

bool exclusion_contains(std::vector<int>* exclusions, int x, int y)
{
	int ex = (x * 64) + y;
	for (int i = 0; i < exclusions->size(); i++)
	{
		if ((*exclusions)[i] == ex)
		{
			return true;
		}
	}

	return false;
}

vector2 get_rand_pos(std::vector<int>* exclusions, random_local* rnd, prop_data* data)
{
	int pos_x = 0;
	int pos_z = 0;

	bool found_value = false;
	while (!found_value)
	{
		pos_x = rnd->range(0, 63);
		pos_z = rnd->range(0, 63);

		bool exists = false;
		for (int b = 0; b < exclusions->size(); b++)
		{
			if ((*exclusions)[b] == (pos_x * 64) + pos_z)
			{
				exists = true;
			}
		}

		if (!exists)
			found_value = true;
	}

	if (data->bounds_x != 0 && data->bounds_y != 0)
	{
		for (int z = -data->bounds_x; z < data->bounds_x; z++)
		{
			for (int w = -data->bounds_y; w < data->bounds_y; w++)
				exclusions->push_back(((pos_x + z) * 64) + (pos_z + w));
		}
	}
	else
	{
		exclusions->push_back((pos_x * 64) + pos_z);
	}

	return vector2(pos_x, pos_z);
}

std::vector<prop_info> generate_props(generate_data* gd, int seed, int scx, int scy, uint16_t* heightmap)
{
	std::vector<prop_info> prop_infos;
	std::vector<int> exclusions;

	scx = scx - 32;
	scy = scy - 32;

	random_local rnd = random_local(seed + (scx * scy) + scx);


	for (int b = 0; b < gd->biomes.size(); b++)
	{
		float l = (noise(&gd->biome_map, seed, scx, scy) + 1) / (float)2;
		if (use_biome(gd->biomes.size(), l, b))
		{
			float weight = round(biome_weight(gd->biomes.size(), l, b) * 10) / 10;
			std::vector<prop_data> prop_datas = gd->biomes[b].props;

			for (int i = 0; i < prop_datas.size(); i++)
			{
				if (prop_datas[i].generate_type == 0)
				{
					int quantity = rnd.range(prop_datas[i].per_chunk_min, prop_datas[i].per_chunk_max);

					for (int j = 0; j < quantity; j++)
					{
						vector2 pos = get_rand_pos(&exclusions, &rnd, &prop_datas[i]);
						int iter = (pos.x * 64) + pos.y;

						prop_infos.push_back(add_prop(&prop_datas[i], &rnd, pos.x, heightmap[iter], pos.y));
					}
				}
				else if (prop_datas[i].generate_type == 1)
				{
					if (rnd.range(0, 255) >= prop_datas[i].chance)
					{
						vector2 pos = get_rand_pos(&exclusions, &rnd, &prop_datas[i]);
						int iter = (pos.x * 64) + pos.y;

						prop_infos.push_back(add_prop(&prop_datas[i], &rnd, pos.x, heightmap[iter], pos.y));
					}
				}
				else if (prop_datas[i].generate_type == 2)
				{
					for (int x = 0; x < 64; x++)
					{
						for (int y = 0; y < 64; y++)
						{
							if ((noise(&prop_datas[i].perlin_map, seed - 4, scx + x, scy + y) + 1) / 2 <= prop_datas[i].perlin && 
								!exclusion_contains(&exclusions, x, y))
							{
								if (prop_datas[i].bounds_x != 0 && prop_datas[i].bounds_y != 0)
								{
									for (int z = -prop_datas[i].bounds_x; z < prop_datas[i].bounds_x; z++)
									{
										for (int w = -prop_datas[i].bounds_y; w < prop_datas[i].bounds_y; w++)
											exclusions.push_back(((x + z) * 64) + (y + w));
									}
								}
								else
								{
									exclusions.push_back((x * 64) + y);
								}

								prop_infos.push_back(add_prop(&prop_datas[i], &rnd, x, heightmap[(x * 64) + y], y));
							}
						}
					}
				}
			}
		}
	}

	return prop_infos;
}

#endif