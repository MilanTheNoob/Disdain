using System;

public class Noise
{
    byte[] perm;

    public unsafe Noise(ulong seed)
    {
        ulong x_ = seed << 1;
        ulong y_ = seed >> 1;

        ulong x = x_, y = y_, temp_x, temp_y, z;

        fixed (byte* pBuffer = perm)
        {
            ulong* pIndex = (ulong*)pBuffer;
            ulong* pEnd = (ulong*)(pBuffer + perm.Length);

            while (pIndex <= pEnd - 1)
            {
                temp_x = y;
                x ^= x << 23; temp_y = x ^ y ^ (x >> 17) ^ (y >> 26);

                *(pIndex++) = temp_y + y;

                x = temp_x;
                y = temp_y;
            }

            if (pIndex < pEnd)
            {
                temp_x = y;
                x ^= x << 23; temp_y = x ^ y ^ (x >> 17) ^ (y >> 26);
                z = temp_y + y;

                byte* pByte = (byte*)pIndex;
                while (pByte < pEnd) *(pByte++) = (byte)(z >>= 8);
            }
        }

        x_ = x;
        y_ = y;
    }

    /// <summary>
    /// 2D simplex noise
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public float Generate(float x, float y)
    {
        const float G2 = 0.211324865f;
        float n0, n1, n2;

        var s = (x + y) * 0.366025403f;
        var xs = x + s;
        var ys = y + s;
        var i = FastFloor(xs);
        var j = FastFloor(ys);

        var t = (i + j) * G2;
        var X0 = i - t;
        var Y0 = j - t;
        var x0 = x - X0;
        var y0 = y - Y0;

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; }
        else { i1 = 0; j1 = 1; }

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1.0f + 2.0f * G2;
        var y2 = y0 - 1.0f + 2.0f * G2;

        var ii = Mod(i, 256);
        var jj = Mod(j, 256);

        var t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0.0f) n0 = 0.0f;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Grad(perm[ii + perm[jj]], x0, y0);
        }

        var t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0.0f) n1 = 0.0f;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Grad(perm[ii + i1 + perm[jj + j1]], x1, y1);
        }

        var t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0.0f) n2 = 0.0f;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Grad(perm[ii + 1 + perm[jj + 1]], x2, y2);
        }

        return 40.0f * (n0 + n1 + n2);
    }

    int FastFloor(float x)
    {
        return (x > 0) ? ((int)x) : (((int)x) - 1);
    }

    int Mod(int x, int m)
    {
        var a = x % m;
        return a < 0 ? a + m : a;
    }

    float Grad(int hash, float x, float y)
    {
        var h = hash & 7;
        var u = h < 4 ? x : y;
        var v = h < 4 ? y : x;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }
}