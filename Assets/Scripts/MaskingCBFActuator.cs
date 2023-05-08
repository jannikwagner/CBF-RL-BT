using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class CBFDiscreteInvalidActionMasker
{
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask, CBFApplicator[] cbfApplicators, int numActions)
    {
        bool[] actionMasked = new bool[numActions];
        foreach (var cbfApplicator in cbfApplicators)
        {
            bool[] actionMaskedNew = new bool[numActions];
            var allMasked = true;
            for (int i = 0; i < numActions; i++)
            {
                if (cbfApplicator.debug) Debug.Log("Action: " + i);
                var actions = new ActionBuffers(new float[] { }, new int[] { i });
                var okay = cbfApplicator.isActionValid(actions);
                bool mask = !okay || actionMasked[i];
                actionMaskedNew[i] = mask;
                allMasked = allMasked && mask;
            }
            if (allMasked)
            {
                if (cbfApplicator.debug) Debug.Log("All actions masked! CBF: " + cbfApplicator.cbf);
                break;
            }
            actionMasked = actionMaskedNew;
        }
        for (int i = 0; i < numActions; i++)
        {
            actionMask.SetActionEnabled(0, i, !actionMasked[i]);
        }
    }
}

/* I would like to let my Actuators inherit from this class,
however at the time when the actuators are created, the cbfAppliers are not yet created.
Those are created in Start of the agent or enviroment, because they require the Unity GameObjects to be loaded.*/

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
