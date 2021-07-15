#include "Packet.h"

#ifndef SAVEFILE_H
#define SAVEFILE_H

struct SaveFileData
{
public:
	SaveFileData(Packet packet) {}
	SaveFileData() {}
};

struct ChunkData
{
public:
	int x;
	int y;

	uint16_t* heightmap;

	ChunkData() {}
	ChunkData(uint16_t _heightmap[], int _x, int _y) 
	{
		heightmap = _heightmap;
		
		x = _x;
		y = _y;
	}

	void Dispose()
	{
		delete[] heightmap;
	}
};

#endif