using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICBF
{
    public float evaluate(float[] x);
    public float[] gradient(float[] x);
}

public class MovingBallCBF3D : ICBF
{
    public float radius;

    public MovingBallCBF3D(float radius)
    {
        this.radius = radius;
    }

    public float evaluate(float[] x)
    {
        var position = new Vector3(x[0], x[1], x[2]);
        var center = new Vector3(x[3], x[4], x[5]);
        return (position - center).magnitude - radius;
    }

    public float[] gradient(float[] x)
    {
        var position = new Vector3(x[0], x[1], x[2]);
        var center = new Vector3(x[3], x[4], x[5]);
        var diff = (position - center).normalized;
        return new float[] { diff.x, diff.y, diff.z, -diff.x, -diff.y, -diff.z };
    }
}

public class StaticBallCBF3D : ICBF
{
    public float radius;
    public Vector3 center;

    public StaticBallCBF3D(float radius, Vector3 center)
    {
        this.radius = radius;
        this.center = center;
    }

    public float evaluate(float[] x)
    {
        return (Utility.ArrToVec3(x) - center).magnitude - radius;
    }

    public float[] gradient(float[] x)
    {
        return Utility.vec3ToArr((Utility.ArrToVec3(x) - center).normalized);
    }
}

public class WallCBF3D : ICBF
{
    public Vector3 point;
    public Vector3 normal;

    public WallCBF3D(Vector3 point, Vector3 normal)
    {
        this.point = point;
        this.normal = normal;
    }

    public float evaluate(float[] x)
    {
        // Debug.Log("x: " + Utility.ArrToVec3(x) + ", point: " + point.ToString() + ", normal: " + normal.ToString());
        return Vector3.Dot(Utility.ArrToVec3(x) - point, normal);
    }

    public float[] gradient(float[] x)
    {
        return Utility.vec3ToArr(normal);
    }
}

public class StaticBatteryMarginCBF : ICBF
{
    public float margin;
    public Vector3 center;
    public float batteryConsumption;

    public StaticBatteryMarginCBF(Vector3 center, float margin, float batteryConsumption)
    {
        this.center = center;
        this.margin = margin;
        this.batteryConsumption = batteryConsumption;
    }

    public float evaluate(float[] x)
    {
        var position = new Vector3(x[0], x[1], x[2]);
        var battery = x[3];
        return battery - ((position - center).magnitude + margin) * batteryConsumption;
    }

    public float[] gradient(float[] x)
    {
        var position = new Vector3(x[0], x[1], x[2]);
        var battery = x[3];
        var diff = -(position - center).normalized * batteryConsumption;
        return new float[] { diff.x, diff.y, diff.z, 1f };
    }
}
