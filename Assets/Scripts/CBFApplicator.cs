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
    protected IDynamicsProvider dynamicsProvider;
    public ICBF cbf;
    public bool debug;
    public float delta_t;

    public CBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float delta_t, bool debug = false)
    {
        this.cbf = cbf;
        this.dynamicsProvider = controlledDynamics;
        this.debug = debug;
        this.delta_t = delta_t;
    }

    public bool isSafe()
    {
        return isSafe(dynamicsProvider.x());
    }

    bool isSafe(float[] x)
    {
        return cbf.h(x) >= 0;
    }

    public float evaluate()
    {
        return cbf.h(dynamicsProvider.x());
    }

    public float[] gradient()
    {
        return cbf.dhdx(dynamicsProvider.x());
    }

    public abstract bool isActionValid(ActionBuffers action);
}

public class ContinuousCBFApplicator : CBFApplicator
{
    Func<float, float> alpha = (x) => x;

    public ContinuousCBFApplicator(ICBF cbf, IDynamicsProvider dynamicsProvider, float delta_t, Func<float, float> alpha = null, bool debug = false) : base(cbf, dynamicsProvider, delta_t, debug)
    {
        if (alpha != null)
        {
            this.alpha = alpha;
        }
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x = dynamicsProvider.x();
        var dhdx = cbf.dhdx(x);
        // var dxdt = dynamicsProvider.dxdt(action);
        // var dhdt = Utility.Dot(dxdt, gradient);
        // float delta_h = deltaTime * dhdt; // how it was previously done, this assumes that the dynamics are first order and effectively sets alpha to 1/deltaTime
        // var delta_x = Utility.Mult(dxdt, deltaTime)  // equivalent to how it was previously done, this assumes that the dynamics are first order
        var delta_x = dynamicsProvider.delta_x(action, delta_t);  // better approximation of the change in state, allows for 2nd order dynamics
        var delta_h = Utility.Dot(delta_x, dhdx);  // better approximation of the change in h
        float left = delta_h;
        float right = -alpha.Invoke(cbf.h(x));
        return left >= right;
    }
}
public class DiscreteCBFApplicator : CBFApplicator
{
    private float eta;
    public DiscreteCBFApplicator(ICBF cbf, IDynamicsProvider dynamicsProvider, float delta_t, float eta = 1, bool debug = false) : base(cbf, dynamicsProvider, delta_t, debug)
    /*
     * eta: How strongly the state should be pushed into the safe set
     * deltaTime: the time step
     */
    {
        this.eta = eta;
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x1 = dynamicsProvider.x();
        // var dxdt = dynamicsProvider.dxdt(action);
        // var delta_x = Utility.Mult(dxdt, deltaTime)  // how it was previously done, this assumes that the dynamics are first order
        var delta_x = dynamicsProvider.delta_x(action, delta_t);  // better approximation of the change in state, allows for 2nd order dynamics
        var x2 = Utility.Add(x1, delta_x);
        var h2 = cbf.h(x2);
        var h1 = cbf.h(x1);
        var criterion = h2 + (eta - 1) * h1;
        return criterion >= 0;
    }
}
