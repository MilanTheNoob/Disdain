#ifndef NOISE_H
#define NOISE_H

#include "generate_data.h"

const float gradients[] =
{
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.38268343236509f, 0.923879532511287f, 0.923879532511287f, 0.38268343236509f, 0.923879532511287f, -0.38268343236509f, 0.38268343236509f, -0.923879532511287f,
    -0.38268343236509f, -0.923879532511287f, -0.923879532511287f, -0.38268343236509f, -0.923879532511287f, 0.38268343236509f, -0.38268343236509f, 0.923879532511287f,
};

const int prime_x = 501125321;
const int prime_y = 1136930381;

float grad_coord(int seed, int xPrimed, int yPrimed, float xd, float yd)
{
    int hash = seed ^ xPrimed ^ yPrimed;
    hash *= 0x27d4eb2d;

    hash ^= hash >> 15;
    hash &= 127 << 1;

    float xg = gradients[hash];
    float yg = gradients[hash | 1];

    return xd * xg + yd * yg;
}

float single_simplex(int seed, float x, float y)
{
    const float SQRT3 = 1.7320508075688772935274463415059f;
    const float G2 = (3 - SQRT3) / 6;

    const float F2 = 0.5f * (SQRT3 - 1);
    float s = (x + y) * F2;
    x += s; y += s;

    int i = x >= 0 ? (int)x : (int)x - 1;
    int j = y >= 0 ? (int)y : (int)y - 1;
    float xi = (float)(x - i);
    float yi = (float)(y - j);

    float t = (xi + yi) * G2;
    float x0 = (float)(xi - t);
    float y0 = (float)(yi - t);

    i *= prime_x;
    j *= prime_y;

    float n0, n1, n2;

    float a = 0.5f - x0 * x0 - y0 * y0;
    if (a <= 0) n0 = 0;
    else
    {
        n0 = (a * a) * (a * a) * grad_coord(seed, i, j, x0, y0);
    }

    float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
    if (c <= 0) n2 = 0;
    else
    {
        float x2 = x0 + (2 * (float)G2 - 1);
        float y2 = y0 + (2 * (float)G2 - 1);
        n2 = (c * c) * (c * c) * grad_coord(seed, i + prime_x, j + prime_y, x2, y2);
    }

    if (y0 > x0)
    {
        float x1 = x0 + (float)G2;
        float y1 = y0 + ((float)G2 - 1);
        float b = 0.5f - x1 * x1 - y1 * y1;
        if (b <= 0) n1 = 0;
        else
        {
            n1 = (b * b) * (b * b) * grad_coord(seed, i, j + prime_y, x1, y1);
        }
    }
    else
    {
        float x1 = x0 + ((float)G2 - 1);
        float y1 = y0 + (float)G2;
        float b = 0.5f - x1 * x1 - y1 * y1;
        if (b <= 0) n1 = 0;
        else
        {
            n1 = (b * b) * (b * b) * grad_coord(seed, i + prime_x, j, x1, y1);
        }
    }

    return (n0 + n1 + n2) * 99.83685446303647f;
}

float noise(heightmap_data* data, int seed, float x, float y)
{
    float noise_height = 0;

    float amplitude = 1;
    float frequency = 1;

    for (int i = 0; i < data->octaves; i++)
    {
        noise_height = noise_height + single_simplex(seed * (i + 1),
            x / data->scale * frequency,
            y / data->scale * frequency) * amplitude;

        amplitude = amplitude * data->persistence;
        frequency = frequency * data->lacunarity;
    }

    return (noise_height / data->octaves) * data->height_multiplier;
}

#endif