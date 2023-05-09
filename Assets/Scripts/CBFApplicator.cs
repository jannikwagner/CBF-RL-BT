using System;
using UnityEngine;
using Unity.MLAgents.Actuators;

public abstract class CBFApplicator
{
    protected IDynamicsProvider controlledDynamics;
    public ICBF cbf;
    public bool debug;

    public CBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, bool debug = false)
    {
        this.cbf = cbf;
        this.controlledDynamics = controlledDynamics;
        this.debug = debug;
    }

    public bool isSafe()
    {
        return isSafe(controlledDynamics.x());
    }

    public bool isSafe(float[] x)
    {
        return cbf.evaluate(x) >= 0;
    }

    public float evluate()
    {
        return cbf.evaluate(controlledDynamics.x());
    }

    public float[] gradient()
    {
        return cbf.gradient(controlledDynamics.x());
    }

    public abstract bool isActionValid(ActionBuffers action);
}

public class ContinuousCBFApplicator : CBFApplicator
{
    Func<float, float> alpha = (x) => x;

    public ContinuousCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, Func<float, float> alpha = null, bool debug = false) : base(cbf, controlledDynamics, debug)
    {
        if (alpha != null)
        {
            this.alpha = alpha;
        }
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x = controlledDynamics.x();
        var gradient = cbf.gradient(x);
        var dxdt = controlledDynamics.dxdt(action);
        bool debugLocal = debug && (action.DiscreteActions[0] == 14 || action.DiscreteActions[0] == 10);
        if (debugLocal) Debug.Log("action: " + action.DiscreteActions[0]);
        if (debugLocal) Debug.Log("dxdt: " + Utility.arrToStr(dxdt));
        if (debugLocal) Debug.Log("dhdx: " + Utility.arrToStr(gradient));
        if (debugLocal) Debug.Log("x: " + Utility.arrToStr(x));
        float left = Utility.Dot(dxdt, gradient);
        float right = -alpha.Invoke(cbf.evaluate(x));
        if (debugLocal) Debug.Log("left: " + left + ", right: " + right);
        return left >= right;
    }
}
public class DiscreteCBFApplicator : CBFApplicator
{
    private float eta;
    private float deltaTime;
    public DiscreteCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float eta, float deltaTime, bool debug = false) : base(cbf, controlledDynamics, debug)
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
        var currentState = controlledDynamics.x();
        var dynamics = controlledDynamics.dxdt(action);
        var nextState = Utility.Add(currentState, Utility.Mult(dynamics, deltaTime));
        var nextValue = cbf.evaluate(nextState);
        var currentValue = cbf.evaluate(currentState);
        var criterion = nextValue + (eta - 1) * currentValue;
        return criterion >= 0;
    }
}
