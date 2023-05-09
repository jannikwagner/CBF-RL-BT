using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class Utility
{
    public static float eps => 1e-5f;
    public static float[] vec3ToArr(Vector3 vec)
    {
        return new float[] { vec.x, vec.y, vec.z };
    }

    public static Vector3 ArrToVec3(float[] arr, int start = 0)
    {
        return new Vector3(arr[0 + start], arr[1 + start], arr[2 + start]);
    }

    public static float[] Concat(in float[][] vectors)
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
    public static float[] Concat(in float[] vector1, in float[] vector2)
    {
        return Concat(new float[][] { vector1, vector2 });
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

    public static String arrToStr<T>(IEnumerable<T> arr)
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

    public static float Round(float value, int digits)
    {
        var mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }

    internal static float[] Concat(Vector3 vec1, Vector3 vec2)
    {
        return Concat(new Vector3[] { vec1, vec2 });
    }

    private static float[] Concat(Vector3[] vector3s)
    {
        return Concat(vector3s.Select(vec => vec3ToArr(vec)).ToArray());
    }
}

// @see https://stackoverflow.com/questions/35461643/what-is-faster-in-finding-element-with-property-of-maximum-value
public static class EnumerableExtensions
{
    public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new InvalidOperationException();

            var max = iterator.Current;
            var maxValue = selector(max);
            var comparer = Comparer<float>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, maxValue) > 0)
                {
                    max = current;
                    maxValue = currentValue;
                }
            }

            return max;
        }
    }
    public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
        return source.MaxBy((element) => -selector(element));
    }
}
