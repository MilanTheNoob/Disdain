#include <cstdint>

#ifndef MATH_H
#define MATH_H

static int clamp(int n, int min, int max) 
{
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

static uint8_t clamp(uint8_t n, uint8_t min, uint8_t max)
{
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

static float clamp(float n, float min, float max)
{
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

#endif