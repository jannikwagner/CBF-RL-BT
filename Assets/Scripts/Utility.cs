using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static float[] vec3ToArr(Vector3 vec)
    {
        return new float[] { vec.x, vec.y, vec.z };
    }

    public static Vector3 ArrToVec3(float[] arr)
    {
        return new Vector3(arr[0], arr[1], arr[2]);
    }

    public static float[] combineArrs(in float[][] vectors)
    {
        var length = 0;
        foreach (var vector in vectors)
        {
            length += vector.Length;
        }

        var result = new float[length];
        var index = 0;
        foreach (var vector in vectors)
        {
            foreach (var element in vector)
            {
                result[index] = element;
                index++;
            }
        }
        return result;
    }
    public static float[] combineArrs(in float[] vector1, in float[] vector2)
    {
        return combineArrs(new float[][] { vector1, vector2 });
    }

    public static float Dot(in float[] vector1, in float[] vector2)
    {
        var result = 0f;
        for (int i = 0; i < vector1.Length; i++)
        {
            result += vector1[i] * vector2[i];
        }
        return result;
    }

    public static float[] Add(in float[] vector1, in float[] vector2)
    {
        var result = new float[vector1.Length];
        for (int i = 0; i < vector1.Length; i++)
        {
            result[i] = vector1[i] + vector2[i];
        }
        return result;
    }

    internal static float[] Mult(float[] floats, float factor)
    {
        var result = new float[floats.Length];
        for (int i = 0; i < floats.Length; i++)
        {
            result[i] = floats[i] * factor;
        }
        return result;
    }
}