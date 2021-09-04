#ifndef RANDOM_H
#define RANDOM_H

class random_local
{
private:
    int seed;
public:
    random_local(int _seed) { seed = uint64_t(_seed) << 31 | uint64_t(_seed); }

    uint32_t next()
    {
        uint64_t result = seed * 0xd989bcacc137dcd5ull;
        seed ^= seed >> 11;
        seed ^= seed << 31;
        seed ^= seed >> 18;
        return uint32_t(result >> 32ull);
    }

    int range(int min, int max) { return next() % (max + 1 - min) + min; }
};

#endif