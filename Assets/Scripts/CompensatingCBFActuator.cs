using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class CBFCompensator
{
    private ActionSpec actionSpec;
    public CBFCompensator(ActionSpec actionSpec)
    {
        this.actionSpec = actionSpec;
    }
    /// <summary>
    /// This method enforces the agent to respect the CBF by compensating the action in a minimally intrusive way.
    /// The compensation is the result of a quadratic program that minimizes the distance to the original action.
    /// The constraints for the quadratic program are the CBFs.
    /// </summary>
    public void Compensate(ActionBuffers action, CBFApplicator[] cbfAppliers)
    {

    }
}

// public abstract class MaskingCBFActuator : IActuator
// {
//     private CBFDiscreteInvalidActionMasker masker;
//     private CBFApplicator[] cbfAppliers;
//     public MaskingCBFActuator(CBFApplicator[] cbfAppliers)
//     {
//         this.masker = new CBFDiscreteInvalidActionMasker(this.ActionSpec);
//         this.cbfAppliers = cbfAppliers;
//     }
//     public abstract ActionSpec ActionSpec { get; }
//     public abstract string Name { get; }
//     public abstract void Heuristic(in ActionBuffers actionBuffersOut);
//     public abstract void OnActionReceived(ActionBuffers actionBuffers);
//     public abstract void ResetData();
//     public virtual void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
//     {
//         masker.WriteDiscreteActionMask(actionMask, cbfAppliers);
//     }
// }
