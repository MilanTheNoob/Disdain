#ifndef WEIGHTING_H
#define WEIGHTING_H

#include <cstdlib>
#include "math.h"

float biome_weight(float biomes_count, float rand_num, float biome)
{
	return clamp(-abs(biomes_count * rand_num - biome) + 1, (float)0, (float)1);
}

bool use_biome(int biomes_count, float rand_num, int biome)
{
	if ((biome - (float)1) / biomes_count <= rand_num && rand_num <= (biome + (float)1) / biomes_count) { return true; } return false;
}

#endif
