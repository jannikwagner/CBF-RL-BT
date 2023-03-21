using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;


public interface IStateProvider
{
    public float[] currentState();
    public float[] Dynamics();
}

public interface IStateController
{
    public float[] currentState();
    public float[] ControlledDynamics(ActionBuffers action);
}

public class CombinedState : IStateController
{
    public IStateController controlledState;
    public IStateProvider state;

    public CombinedState(IStateController controlledState, IStateProvider state)
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
