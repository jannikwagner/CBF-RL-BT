using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;


public interface IDynamics
{
    public float[] currentState();
    public float[] Dynamics();
}

public interface IControlledDynamics
{
    public float[] currentState();
    public float[] ControlledDynamics(ActionBuffers action);
}

public class CombinedDynamics : IControlledDynamics
{
    public IControlledDynamics controlledState;
    public IDynamics state;

    public CombinedDynamics(IControlledDynamics controlledState, IDynamics state)
    {
        this.controlledState = controlledState;
        this.state = state;
    }

    public float[] currentState()
    {
        return Utility.combineArrs(controlledState.currentState(), state.currentState());
    }

    public float[] ControlledDynamics(ActionBuffers action)
    {
        return Utility.combineArrs(controlledState.ControlledDynamics(action), state.Dynamics());
    }
}
