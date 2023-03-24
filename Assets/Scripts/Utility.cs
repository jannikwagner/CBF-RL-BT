using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public static float[] Mult(float[] floats, float factor)
    {
        var result = new float[floats.Length];
        for (int i = 0; i < floats.Length; i++)
        {
            result[i] = floats[i] * factor;
        }
        return result;
    }

    public static String arrToStr(float[] arr)
    {
        var str = "[";
        foreach (var element in arr)
        {
            str += element + ", ";
        }
        str += "]";
        return str;
    }

    public static Vector3 SamplePosition(float xMin,
                                  float xMax,
                                  float zMin,
                                  float zMax,
                                  float yMin,
                                  float yMax,
                                  float minDistance,
                                  Vector3[] positionsToAvoid)
    {
        Vector3 samplePosition;
        while (true)
        {
            samplePosition = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax));

            var anyTooClose = false;
            foreach (var positionToAvoid in positionsToAvoid)
            {
                if ((samplePosition - positionToAvoid).magnitude < minDistance)
                {
                    anyTooClose = true;
                    break;
                }
            }
            if (!anyTooClose)
            {
                break;
            }
        }
        return samplePosition;
    }

}