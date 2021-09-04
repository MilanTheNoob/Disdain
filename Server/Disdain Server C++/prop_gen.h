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
};

prop_info add_prop(prop_data* data, random_local* rnd, int x_pos, int y_pos, int z_pos)
{
	std::cout << "PROP" << std::endl;
	prop_info prop;

	prop.group_id = data->group_id;
	prop.prop_id = data->prop_id;

	prop.pos_x = x_pos;
	prop.pos_y = y_pos;
	prop.pos_z = z_pos;

	prop.rotate_x = rnd->range(-data->rotate_x, data->rotate_x);
	prop.rotate_y = rnd->range(-data->rotate_y, data->rotate_y);
	prop.rotate_z = rnd->range(-data->rotate_z, data->rotate_z);

	prop.scale = rnd->range(-data->min_scale, data->max_scale) * (float)10;

	return prop;
}

std::vector<prop_info> generate_props(generate_data* gd, int seed, int scx, int scy, uint16_t* heightmap)
{
	std::vector<prop_info> prop_infos;

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
					int quantity = rnd.range(prop_datas[i].per_chunk_min, prop_datas[i].per_chunk_max) * weight;

					for (int j = 0; j < quantity; j++)
					{
						int pos_x = rnd.range(-32, 32);
						int pos_z = rnd.range(-32, 32);

						prop_infos.push_back(add_prop(&prop_datas[i], &rnd, pos_x, heightmap[(pos_x * 64) + pos_z], pos_z));
					}
				}
				else if (prop_datas[i].generate_type == 1)
				{
					if (((rand() % 1) * weight) >= prop_datas[i].chance)
					{
						int pos_x = rnd.range(-32, 32);
						int pos_z = rnd.range(-32, 32);

						prop_infos.push_back(add_prop(&prop_datas[i], &rnd, pos_x, heightmap[(pos_x * 64) + pos_z], pos_z));
					}
				}
				else if (prop_datas[i].generate_type == 2)
				{
					for (int x = 0; x < 64; x++)
					{
						for (int y = 0; y < 64; y++)
						{
							if ((noise(&prop_datas[i].perlin_map, seed - 4, scx + x, scy + y) * weight) <= prop_datas[i].perlin)
								prop_infos.push_back(add_prop(&prop_datas[i], &rnd, x, heightmap[(x * 64) + y], y));
						}
					}
				}
			}
		}
	}

	return prop_infos;
}

#endif