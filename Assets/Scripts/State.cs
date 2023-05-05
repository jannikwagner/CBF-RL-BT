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

public interface IControlledDynamics
{
    public float[] currentState();
    public float[] ControlledDynamics(ActionBuffers action);
}

public class CombinedDynamics : IControlledDynamics
{
    public IControlledDynamics controlledState1;
    public IControlledDynamics controlledState2;

    public CombinedDynamics(IControlledDynamics controlledState1, IControlledDynamics controlledState2)
    {
        this.controlledState1 = controlledState1;
        this.controlledState2 = controlledState2;
    }

    public float[] currentState()
    {
        return Utility.Concat(controlledState1.currentState(), controlledState2.currentState());
    }

    public float[] ControlledDynamics(ActionBuffers action)
    {
        return Utility.Concat(controlledState1.ControlledDynamics(action), controlledState2.ControlledDynamics(action));
    }
}
