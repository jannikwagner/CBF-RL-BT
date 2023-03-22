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
    public IControlledDynamics controlledState;
    public bool debug;

    public CBFApplicator(ICBF cbf, IControlledDynamics controlledState, bool debug = false)
    {
        this.cbf = cbf;
        this.controlledState = controlledState;
        this.debug = debug;
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
        var x = controlledState.currentState();
        var gradient = cbf.gradient(x);
        var dynamics = controlledState.ControlledDynamics(action);
        if (debug) Debug.Log("dynamics: " + Utility.arrToStr(dynamics));
        if (debug) Debug.Log("gradient: " + Utility.arrToStr(gradient));
        if (debug) Debug.Log("State: " + Utility.arrToStr(x));
        float left = Utility.Dot(dynamics, gradient) * Time.deltaTime * steps;
        float right = -cbf.evaluate(x);
        if (debug) Debug.Log("left: " + left + ", right: " + right);
        return left >= right;
    }

    public bool actionOkayDiscrete(ActionBuffers action, float steps = 1f)
    {
        var nextState = Utility.Add(controlledState.currentState(), Utility.Mult(controlledState.ControlledDynamics(action), Time.deltaTime * steps));
        return isSafe(nextState);
    }
}
