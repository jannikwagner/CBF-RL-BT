using System;
using UnityEngine;
using Unity.MLAgents.Actuators;

public interface ICBFApplicator
{
    float evaluate();
    float[] gradient();
    bool isActionValid(ActionBuffers action);
    bool isSafe();
}

public abstract class CBFApplicator : ICBFApplicator
{
    protected IDynamicsProvider controlledDynamics;
    public ICBF cbf;
    public bool debug;
    public float deltaTime;

    public CBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, bool debug = false)
    {
        this.cbf = cbf;
        this.controlledDynamics = controlledDynamics;
        this.debug = debug;
        this.deltaTime = deltaTime;
    }

    public bool isSafe()
    {
        return isSafe(controlledDynamics.x());
    }

    bool isSafe(float[] x)
    {
        return cbf.h(x) >= 0;
    }

    public float evaluate()
    {
        return cbf.h(controlledDynamics.x());
    }

    public float[] gradient()
    {
        return cbf.dhdx(controlledDynamics.x());
    }

    public abstract bool isActionValid(ActionBuffers action);
}

public class ContinuousCBFApplicator : CBFApplicator
{
    Func<float, float> alpha = (x) => x;

    public ContinuousCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, Func<float, float> alpha = null, bool debug = false) : base(cbf, controlledDynamics, deltaTime, debug)
    {
        if (alpha != null)
        {
            this.alpha = alpha;
        }
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x = controlledDynamics.x();
        var gradient = cbf.dhdx(x);
        var dxdt = controlledDynamics.dxdt(action);
        bool debugLocal = debug;// && (action.DiscreteActions[0] == 14 || action.DiscreteActions[0] == 10);
        if (debugLocal) Debug.Log("action: " + action.DiscreteActions[0]);
        if (debugLocal) Debug.Log("dxdt: " + Utility.arrToStr(dxdt));
        if (debugLocal) Debug.Log("dhdx: " + Utility.arrToStr(gradient));
        if (debugLocal) Debug.Log("x: " + Utility.arrToStr(x));
        float left = deltaTime * Utility.Dot(dxdt, gradient);
        float right = -alpha.Invoke(cbf.h(x));
        if (debugLocal) Debug.Log("left: " + left + ", right: " + right);
        return left >= right;
    }
}
public class DiscreteCBFApplicator : CBFApplicator
{
    private float eta;
    public DiscreteCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, float eta = 0, bool debug = false) : base(cbf, controlledDynamics, deltaTime, debug)
    /*
     * eta: How strongly the state should be pushed into the safe set
     * deltaTime: the time step
     */
    {
        this.eta = eta;
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var currentState = controlledDynamics.x();
        var dynamics = controlledDynamics.dxdt(action);
        var nextState = Utility.Add(currentState, Utility.Mult(dynamics, deltaTime));
        var nextValue = cbf.h(nextState);
        var currentValue = cbf.h(currentState);
        var criterion = nextValue + (eta - 1) * currentValue;
        return criterion >= 0;
    }
}
