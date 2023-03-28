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
    private CBFDiscreteInvalidActionMasker masker;
    public Env4Actuator(Env4Agent agent)
    {
        this.agent = agent;
        m_ActionSpec = ActionSpec.MakeDiscrete(25);
        this.masker = new CBFDiscreteInvalidActionMasker(this.ActionSpec);
    }

    public ActionSpec ActionSpec => m_ActionSpec;

    public string Name => "Env4AgentActuator";

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

    public void ResetData() { }

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        masker.WriteDiscreteActionMask(actionMask, agent.cbfApplicators);
    }
}

public class Env4ActuatorComponent : ActuatorComponent
{
    public Env4Agent controller;
    ActionSpec m_ActionSpec = ActionSpec.MakeDiscrete(25);

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new Env4Actuator(controller) };
    }

    public override ActionSpec ActionSpec => m_ActionSpec;
}
