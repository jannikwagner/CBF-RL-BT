using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public abstract class State
{
    public abstract float[] array { get; }
    public abstract State FromArray(float[] array);
    public abstract string ToStr();
}

public class PosVelState
{
    public Vector3 position;
    public Vector3 velocity;
    public static PosVelState FromArray(float[] x)
    {
        return new PosVelState { position = Utility.ArrToVec3(x), velocity = Utility.ArrToVec3(x, 3) };
    }
    public float[] ToArray()
    {
        return Utility.Concat(this.position, this.velocity);
    }
}

public interface IDynamicsProvider
{
    public float[] x();
    public float[] dxdt(ActionBuffers action);
}

public class CombinedDynamics : IDynamicsProvider
{
    public IDynamicsProvider dynamics1;
    public IDynamicsProvider dynamics2;

    public CombinedDynamics(IDynamicsProvider dynamics1, IDynamicsProvider dynamics2)
    {
        this.dynamics1 = dynamics1;
        this.dynamics2 = dynamics2;
    }

    public float[] x()
    {
        return Utility.Concat(dynamics1.x(), dynamics2.x());
    }

    public float[] dxdt(ActionBuffers action)
    {
        return Utility.Concat(dynamics1.dxdt(action), dynamics2.dxdt(action));
    }
}
