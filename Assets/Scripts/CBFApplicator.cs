using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

// public interface ICBFApplicator
// {
//     public bool isSafe();
//     public bool actionOkayContinuous(ActionBuffers action);
//     public bool actionOkayDiscrete(ActionBuffers action);
// }

public class CBFApplicator
{
    public ICBF cbf;
    public IStateController controlledState;

    public CBFApplicator(ICBF cbf, IStateController controlledState)
    {
        this.cbf = cbf;
        this.controlledState = controlledState;
    }

    public bool isSafe()
    {
        return isSafe(controlledState.currentState());
    }

    private bool isSafe(float[] x)
    {
        return cbf.evaluate(x) >= 0;
    }

    public float evluate()
    {
        return cbf.evaluate(controlledState.currentState());
    }

    public float[] gradient()
    {
        return cbf.gradient(controlledState.currentState());
    }

    public bool actionOkayContinuous(ActionBuffers action, float steps = 1f)
    {
        var dynamics = controlledState.ControlledDynamics(action);
        // Debug.Log("dynamics: " + Utility.ArrToVec3(dynamics));
        float left = Utility.Dot(dynamics, cbf.gradient(controlledState.currentState())) * Time.deltaTime * steps;
        float right = -cbf.evaluate(controlledState.currentState());
        // Debug.Log("left: " + left + ", right: " + right);
        return left >= right;
    }

    public bool actionOkayDiscrete(ActionBuffers action, float steps = 1f)
    {
        var nextState = Utility.Add(controlledState.currentState(), Utility.Mult(controlledState.ControlledDynamics(action), Time.deltaTime * steps));
        return isSafe(nextState);
    }
}
