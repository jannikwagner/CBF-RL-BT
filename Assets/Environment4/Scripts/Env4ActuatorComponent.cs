using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class Env4Actuator : IActuator
{
    ActionSpec m_ActionSpec;
    private Env4Agent agent;
    public Env4Actuator(Env4Agent agent)
    {
        this.agent = agent;
        m_ActionSpec = ActionSpec.MakeDiscrete(25);
    }

    public ActionSpec ActionSpec => m_ActionSpec;

    public string Name => "Ev4AgentActuator";

    public void Heuristic(in ActionBuffers actionsOut)
    {
        int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
        var discreateActionsOut = actionsOut.DiscreteActions;

        var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
        var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
        discreateActionsOut[0] = i + 5 * j;
        // Debug.Log(discreateActionsOut[0]);
    }

    public void OnActionReceived(ActionBuffers actions)
    {
        Vector3 movement = agent.getMovement(actions);
        agent.Move(movement);
    }


    public void ResetData()
    {
    }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var numActions = ActionSpec.BranchSizes[0];
        if (!agent.useCBF) return;

        bool[] actionMasked = new bool[numActions];
        foreach (var cbfApplicator in agent.cbfApplicators)
        {
            // Debug.Log("CBF: " + cbfApplicator.cbf);
            // Debug.Log(cbfApplicator.evluate());
            bool[] actionMaskedNew = new bool[numActions];
            var allMasked = true;
            for (int i = 0; i < numActions; i++)
            {
                if (cbfApplicator.debug) Debug.Log("Action: " + i);
                var actions = new ActionBuffers(new float[] { }, new int[] { i });
                var okay = cbfApplicator.actionOkayContinuous(actions, agent.decisionRequester.DecisionPeriod);
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
        // Debug.Log("Local position: " + transform.localPosition);
        // Debug.Log("State: " + Utility.ArrToVec3(this.currentState()));
    }
}

public class Env4ActuatorComponent : ActuatorComponent
{
    public Env4Agent controller;
    ActionSpec m_ActionSpec = ActionSpec.MakeDiscrete(25);

    /// <summary>
    /// Creates a BasicActuator.
    /// </summary>
    /// <returns></returns>
    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new Env4Actuator(controller) };
    }

    public override ActionSpec ActionSpec
    {
        get { return m_ActionSpec; }
    }
}