using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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

public class FuncWithDerivative
{
    public Func<float, float> f;
    public Func<float, float> df;

    public FuncWithDerivative(Func<float, float> f, Func<float, float> df)
    {
        this.f = f;
        this.df = df;
    }
}

public class ModulatedCBF : ICBF
{
    public ICBF cbf;
    public FuncWithDerivative alpha;

    public ModulatedCBF(ICBF cbf, FuncWithDerivative alpha)
    {
        this.cbf = cbf;
        this.alpha = alpha;
    }

    public float evaluate(float[] x)
    {
        return alpha.f(cbf.evaluate(x));
    }

    public float[] gradient(float[] x)
    {
        var cbfValue = cbf.evaluate(x);
        var cbfGradient = cbf.gradient(x);
        var alphaDerivative = alpha.df(cbfValue);

        return Utility.Mult(cbfGradient, alphaDerivative);
    }
}

public class SQRTCBF : ModulatedCBF
{
    public SQRTCBF(ICBF cbf) : base(cbf, new FuncWithDerivative(
        (x) => Mathf.Sqrt(x),
        (x) => 0.5f / Mathf.Sqrt(x)
    ))
    { }
}

public class SigmoidCBF : ModulatedCBF
{
    public SigmoidCBF(ICBF cbf) : base(cbf, new FuncWithDerivative(
        (x) => 1f / (1f + Mathf.Exp(-x)),
        (x) => Mathf.Exp(-x) / Mathf.Pow(1f + Mathf.Exp(-x), 2)
    ))
    { }
}

public class SignedSquareCBF : ModulatedCBF
{
    public SignedSquareCBF(ICBF cbf) : base(cbf, new FuncWithDerivative(
        (x) => Mathf.Sign(x) * x * x,
        (x) => 2f * Mathf.Sign(x) * x
    ))
    { }
}
public class StaticPointCBF3D2ndOrderApproximation : ICBF
{
    public Vector3 point;
    public float minDistance;
    public float maxAccel;
    public StaticPointCBF3D2ndOrderApproximation(Vector3 point, float maxAccel, float minDistance = 0)
    {
        this.point = point;
        this.minDistance = minDistance;
        this.maxAccel = maxAccel;
    }
    public float evaluate(float[] x)
    {
        var data = PosVelState.FromArray(x);
        // the normal is changing! as a consequence, this is actually not a static wall
        var normal = (data.position - point).normalized;
        var wall = new StaticWallCBF3D2ndOrder(point, normal, maxAccel, minDistance);
        return wall.evaluate(x);
    }

    public float[] gradient(float[] x)
    {
        var data = PosVelState.FromArray(x);
        var normal = (data.position - point).normalized;
        var wall = new StaticWallCBF3D2ndOrder(point, normal, maxAccel, minDistance);
        return wall.gradient(x);
    }
}


public class StaticWallCBF3D2ndOrder : ICBF
{
    public Vector3 point;
    public Vector3 normal;
    public float minDistance;
    public float maxAccel;
    public StaticWallCBF3D2ndOrder(Vector3 point, Vector3 normal, float maxAccel, float minDistance = 0)
    {
        this.point = point;
        this.normal = normal;
        this.minDistance = minDistance;
        this.maxAccel = maxAccel;
    }
    public float evaluate(float[] x)
    {
        var data = PosVelState.FromArray(x);
        float p = Vector3.Dot(data.position - point, normal) - minDistance;
        float v = Vector3.Dot(data.velocity, normal);

        float h = p + factor(v) * v * v / (2f * maxAccel);
        return h;
    }

    private static float factor(float v)
    {
        return (v < 0) ? -1f : 0f;
        // return Mathf.Sign(v);
    }

    public float[] gradient(float[] x)
    {
        var data = PosVelState.FromArray(x);
        float v = Vector3.Dot(data.velocity, normal);

        float dhdp = 1f;
        float dhdv = 2f * factor(v) * v / (2f * maxAccel);
        float dpdx = normal.x;
        float dpdy = normal.y;
        float dpdz = normal.z;
        float dvdx = normal.x;
        float dvdy = normal.y;
        float dvdz = normal.z;
        return new float[] {
            dhdp * dpdx,
            dhdp * dpdy,
            dhdp * dpdz,
            dhdv * dvdx,
            dhdv * dvdy,
            dhdv * dvdz
        };
    }
}

public class MinCBF : ICBF
{
    public List<ICBF> cbfs;
    public MinCBF(List<ICBF> cbfs)
    {
        this.cbfs = cbfs;
    }

    public float evaluate(float[] x)
    {
        return cbfs.Min(cbf => cbf.evaluate(x));
    }

    public float[] gradient(float[] x)
    {
        return cbfs.MinBy<ICBF>(cbf => cbf.evaluate(x)).gradient(x);
    }
}
public class MaxCBF : ICBF
{
    public List<ICBF> cbfs;
    public MaxCBF(List<ICBF> cbfs)
    {
        this.cbfs = cbfs;
    }

    public float evaluate(float[] x)
    {
        return cbfs.Max(cbf => cbf.evaluate(x));
    }

    public float[] gradient(float[] x)
    {
        return cbfs.MaxBy<ICBF>(cbf => cbf.evaluate(x)).gradient(x);
    }
}
