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
    public float deltaTime;

    public CBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, bool debug = false)
    {
        this.cbf = cbf;
        this.dynamicsProvider = controlledDynamics;
        this.debug = debug;
        this.deltaTime = deltaTime;
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

    public ContinuousCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, Func<float, float> alpha = null, bool debug = false) : base(cbf, controlledDynamics, deltaTime, debug)
    {
        if (alpha != null)
        {
            this.alpha = alpha;
        }
    }

    public override bool isActionValid(ActionBuffers action)
    {
        var x = dynamicsProvider.x();
        var gradient = cbf.dhdx(x);
        var dxdt = dynamicsProvider.dxdt(action);
        bool debugLocal = debug;
        var dhdt = Utility.Dot(dxdt, gradient);
        // float delta_h = deltaTime * dhdt; // how it was previously done
        var delta_x = dynamicsProvider.delta_x(action, deltaTime);
        var delta_h = Utility.Dot(delta_x, gradient);
        float left = delta_h;
        float right = -alpha.Invoke(cbf.h(x));
        return left >= right;
    }
}
public class DiscreteCBFApplicator : CBFApplicator
{
    private float eta;
    public DiscreteCBFApplicator(ICBF cbf, IDynamicsProvider controlledDynamics, float deltaTime, float eta = 1, bool debug = false) : base(cbf, controlledDynamics, deltaTime, debug)
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
        var dxdt = dynamicsProvider.dxdt(action);
        // var delta_x = Utility.Mult(dxdt, deltaTime)  // how it was previously done
        var delta_x = dynamicsProvider.delta_x(action, deltaTime);
        var x2 = Utility.Add(x1, delta_x);
        var h2 = cbf.h(x2);
        var h1 = cbf.h(x1);
        var criterion = h2 + (eta - 1) * h1;
        return criterion >= 0;
    }
}
