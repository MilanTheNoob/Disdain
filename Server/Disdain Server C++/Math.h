#include <cstdint>

#ifndef MATH_H
#define MATH_H

static int Clamp(int n, int min, int max) 
{
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

static int Clamp_8(uint8_t n, uint8_t min, uint8_t max)
{
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

#endif