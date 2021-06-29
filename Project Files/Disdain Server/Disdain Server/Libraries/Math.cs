using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// An optimized proprietary math library with code stolen from Reddit
/// </summary>
public static class Math
{
    #region Clamp Functions

    /// <summary>
    /// Clamps a double between a max & min value
    /// </summary>
    /// <param name="d">The double to clamp</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    public static double Clamp(double d, double min, double max)
    {
        double t = d < min ? min : d;
        return t > max ? max : t;
    }

    /// <summary>
    /// Clamps a float between a max & min value
    /// </summary>
    /// <param name="f">The float to clamp</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    public static float Clamp(float f, float min, float max)
    {
        float t = f < min ? min : f;
        return t > max ? max : t;
    }

    /// <summary>
    /// Clamps an int between a max & min value
    /// </summary>
    /// <param name="i">The double to clamp</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    public static int Clamp(int i, int min, int max)
    {
        int t = i < min ? min : i;
        return t > max ? max : t;
    }

    #endregion
    #region Abs Functions

    /// <summary>
    /// 2x Faster Version of Mathf.Abs which returns an absolute value
    /// </summary>
    /// <param name="i">The integer to perform on</param>
    public static int Abs(int i) { return (i + (i >> 31)) ^ (i >> 31); }
    /// <summary>
    /// 2x Faster Version of Mathf.Abs which returns an absolute value
    /// </summary>
    /// <param name="i">The float to perform on</param>
    public static float Abs(float i) { return i < 0 ? -i : i; }

    #endregion
}
