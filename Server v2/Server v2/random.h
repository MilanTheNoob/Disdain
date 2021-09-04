#include <random>
#ifndef RANDOM_H
#define RANDOM_H

class random_local
{
private:
    int seed;
    std::mt19937* mt;
public:
    random_local(int _seed) 
    {
        std::mt19937 engine(_seed);
        mt = &engine;
    }

    int range(int min, int max)
    {
        std::uniform_int_distribution<int> dist(min, max);

        return dist((*mt));
    }
};

#endif