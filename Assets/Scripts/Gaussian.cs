using UnityEngine;
using System.Collections;
using System;

public static class Gaussian {
    private static System.Random gen = new System.Random();

    public static float GetRandomGaussian() {
        return GetRandomGaussian(0.0f, 1.0f); // standard normal deviation
    }

    public static float GetRandomGaussian(float mean, float stddev)  // overload for single value
    {
        float rVal1, rVal2;

        GetRandomGaussian(mean, stddev, out rVal1, out rVal2);

        return rVal1;
    }

    public static void GetRandomGaussian(float mean, float stddev, out float val1, out float val2) {
        float u, v, s, t;

        do {   // continually generating random values that are uniformly distributed within the bounds [-1, 1]
            u = 2 * (float)gen.NextDouble() - 1;
            v = 2 * (float)gen.NextDouble() - 1;
        } while (u * u + v * v > 1 || (u == 0 && v == 0)); // numbers can't be at origin or on/outside the unit boundary

        s = u * u + v * v;  // equal to radius squared
        t = (float)Math.Sqrt((-2.0f * (float)Math.Log(s)) / s);   // this is why numbers can't be at origin (divide by zero)  

        val1 = stddev * u * t + mean;  // !!! Figure out what's going on here
        val2 = stddev * v * t + mean;
    }
}

