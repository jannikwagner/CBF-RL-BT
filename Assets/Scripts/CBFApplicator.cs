using System;
using UnityEngine;
using Unity.MLAgents.Actuators;

public abstract class CBFApplicator
{
    protected IControlledDynamics controlledDynamics;
    public ICBF cbf;
    public bool debug;

    public CBFApplicator(ICBF cbf, IControlledDynamics controlledDynamics, bool debug = false)
    {
        this.cbf = cbf;
        this.controlledDynamics = controlledDynamics;
        this.debug = debug;
    }

    public bool isSafe()
    {
        return isSafe(controlledDynamics.currentState());
    }

    public bool isSafe(float[] x)
    {
        return cbf.evaluate(x) >= 0;
    }

    public float evluate()
    {
        return cbf.evaluate(controlledDynamics.currentState());
    }

    public float[] gradient()
    {
        return cbf.gradient(controlledDynamics.currentState());
    }

    public abstract bool isActionValid(ActionBuffers action);
}

public class ContinuousCBFApplicator : CBFApplicator
{
    Func<float, float> alpha = (x) => 1f;

    public ContinuousCBFApplicator(ICBF cbf, IControlledDynamics controlledDynamics, Func<float, float> alpha = null, bool debug = false) : base(cbf, controlledDynamics, debug)
    {
        if (alpha != null)
        {
            this.alpha = alpha;
        }
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x = controlledDynamics.currentState();
        var gradient = cbf.gradient(x);
        var dynamics = controlledDynamics.ControlledDynamics(action);
        if (debug) Debug.Log("dynamics: " + Utility.arrToStr(dynamics));
        if (debug) Debug.Log("gradient: " + Utility.arrToStr(gradient));
        if (debug) Debug.Log("State: " + Utility.arrToStr(x));
        float left = Utility.Dot(dynamics, gradient);
        float right = -alpha.Invoke(cbf.evaluate(x));
        if (debug) Debug.Log("left: " + left + ", right: " + right);
        return left >= right;
    }
}
public class DiscreteCBFApplicator : CBFApplicator
{
    private float eta;
    private float deltaTime;
    public DiscreteCBFApplicator(ICBF cbf, IControlledDynamics controlledDynamics, float eta, float deltaTime, bool debug = false) : base(cbf, controlledDynamics, debug)
    /*
     * eta: How strongly the state should be pushed into the safe set
     * deltaTime: the time step
     */
    {
        this.eta = eta;
        this.deltaTime = deltaTime;
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var currentState = controlledDynamics.currentState();
        var dynamics = controlledDynamics.ControlledDynamics(action);
        var nextState = Utility.Add(currentState, Utility.Mult(dynamics, deltaTime));
        var nextValue = cbf.evaluate(nextState);
        var currentValue = cbf.evaluate(currentState);
        var criterion = nextValue + (eta - 1) * currentValue;
        return criterion >= 0;
    }
}
