using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public interface ICBFApplicator
{
    public bool isSafe();
    public bool actionOkayContinuous(ActionBuffers action);
    public bool actionOkayDiscrete(ActionBuffers action);
}

public class CBFApplicator : ICBFApplicator
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

    public bool actionOkayContinuous(ActionBuffers action)
    {
        var control = controlledState.ControlledDynamics(action);
        return Utility.Dot(control, cbf.gradient(controlledState.currentState())) >= -cbf.evaluate(controlledState.currentState());
    }

    public bool actionOkayDiscrete(ActionBuffers action)
    {
        var nextState = Utility.Add(controlledState.currentState(), Utility.Mult(controlledState.ControlledDynamics(action), Time.deltaTime));
        return isSafe(nextState);
    }
}
