using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICBF<T>
{
    public float evaluate(T x);
    public T gradient(T x);
    public bool isSafe(T x)
    {
        return evaluate(x) >= 0;
    }
}

public class MovingBallCBF3D : ICBF<Tuple<Vector3, Vector3>>
{
    public float radius;

    public MovingBallCBF3D(float radius)
    {
        this.radius = radius;
    }

    public float evaluate(Tuple<Vector3, Vector3> x)
    {
        var center = x.Item1;
        var position = x.Item2;
        return (position - center).magnitude - radius;
    }

    public Tuple<Vector3, Vector3> gradient(Tuple<Vector3, Vector3> x)
    {
        var center = x.Item1;
        var position = x.Item2;
        return new Tuple<Vector3, Vector3>((position - center).normalized, (center - position).normalized);
    }
}

public class StaticBallCBF3D : ICBF<Vector3>
{
    public float radius;
    public Vector3 center;

    public StaticBallCBF3D(float radius, Vector3 center)
    {
        this.radius = radius;
        this.center = center;
    }

    public float evaluate(Vector3 x)
    {
        return (x - center).magnitude - radius;
    }

    public Vector3 gradient(Vector3 x)
    {
        return (x - center).normalized;
    }
}

public class WallCBF3D : ICBF<Vector3>
{
    public Vector3 normal;
    public Vector3 point;

    public WallCBF3D(Vector3 normal, Vector3 point)
    {
        this.normal = normal;
        this.point = point;
    }

    public float evaluate(Vector3 x)
    {
        return Vector3.Dot(x - point, normal);
    }

    public Vector3 gradient(Vector3 x)
    {
        return normal;
    }
}
