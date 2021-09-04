#ifndef PLAYER_H
#define PLAYER_H

#include "collision.h"

struct player
{
	bool initialized;
	bool chunk_initialized;

	int player_id;
	SOCKET* socket;

	bool updated;
	bool jump;

	float move_x;
	float move_y;

	float mouse_x;
	float mouse_y;

	collision_object collider;
	int old_chunk;

	vector3 current_pos;
	vector3 old_pos;

	vector2 rotation;
	float velocity;

	int render_distance;
	std::vector<int> rendered_chunks;
	player* viewable_players;
};

#endif // !PLAYER_H
