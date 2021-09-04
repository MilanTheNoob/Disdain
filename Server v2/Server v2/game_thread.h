#ifndef GAME_THREAD_H
#define GAME_THREAD_H

#include "server_data.h"
#include "lobby_collisions.h"

#include <cmath>
#include <chrono>
#include <iostream>

// deals with the sleep func cross compatibility shit
#ifdef __unix
#include <unistd.h>
#define p_sleep(x) sleep((x) / 1000)
#elif defined _WIN32
#include <Windows.h>
#define p_sleep(x) Sleep((x));
#endif

struct game_thread_data
{
	server_data* sdata;
	bool* running;
};

void* game_thread(void* arg)
{
	struct game_thread_data* data;
	data = (struct game_thread_data*)arg;

	while (true)
	{
		uint32_t time = GetTickCount();

		// do stuff here
		lobby_collisions(data->sdata);

		uint32_t delta_time = GetTickCount() - time;
		if (delta_time < 33) p_sleep(33 - delta_time);
	}

	pthread_exit(nullptr);
	return 0;
}
#endif // !GAME_THREAD_H
