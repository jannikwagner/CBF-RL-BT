using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CBF<T, U>
{
    public float evaluate(T x);
    public U gradient(T x);
    public bool isSafe(T x)
    {
        return evaluate(x) >= 0;
    }
}

public class CBF2D : CBF<Vector2, Vector2>
{
    public float radius;
    public Vector2 center;

    public CBF2D(float radius, Vector2 center)
    {
        this.radius = radius;
        this.center = center;
    }

    public float evaluate(Vector2 x)
    {
        return (x - center).magnitude - radius;
    }

    public Vector2 gradient(Vector2 x)
    {
        return (x - center).normalized;
    }
}

public class CBF3D : CBF<Vector3, Vector3>
{
    public float radius;
    public Vector3 center;

    public CBF3D(float radius, Vector3 center)
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
