using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class Env4ActuatorComponent : ActuatorComponent
{
    public Env4Agent agent;
    ActionSpec m_ActionSpec = ActionSpec.MakeDiscrete(25);

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new Env4Actuator(agent) };
    }

    public override ActionSpec ActionSpec => m_ActionSpec;

    public Vector3 GetMovement(ActionBuffers actions, float speed)
    {
        var discreteActions = actions.DiscreteActions;
        var action = discreteActions[0];

        var i = action % 5;
        var j = action / 5;
        var movement = new Vector3(i - 2, 0f, j - 2) * speed / 2.0f;
        return movement;

        // var moveXAction = discreteActions[0];
        // var moveZAction = discreteActions[1];

        // float moveX = moveXAction - 1;
        // float moveZ = moveZAction - 1;

        // var movement = new Vector3(moveX, 0f, moveZ) * speed;
        // return movement;
    }


    public class Env4Actuator : IActuator
    {
        ActionSpec m_ActionSpec = ActionSpec.MakeDiscrete(25);
        private Env4Agent agent;
        private CBFDiscreteInvalidActionMasker masker;
        public Env4Actuator(Env4Agent agent)
        {
            this.agent = agent;
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
            Vector3 movement = agent.actuatorComponent.GetMovement(actions, agent.controller.speed);
            agent.Move(movement);
        }

        public void ResetData() { }

        public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            masker.WriteDiscreteActionMask(actionMask, agent.cbfApplicators);
        }
    }
}
