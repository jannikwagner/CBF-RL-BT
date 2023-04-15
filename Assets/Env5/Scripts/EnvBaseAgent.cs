using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class EnvBaseAgent : BaseAgent
    {
        public PlayerController controller;

        public Vector3 GetForce(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 5;
            var j = action / 5;
            var force = new Vector3(i - 2, 0f, j - 2) / 2.0f;
            return force;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var force = GetForce(actions);
            controller.ApplyForce(force);
            // Debug.Log("BaseAgent.OnActionReceived: " + force);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
            var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
            discreateActionsOut[0] = i + 5 * j;
            // Debug.Log(discreateActionsOut[0]);
        }

        public override void ResetEnv()
        {
            this.controller.env.Initialize();
        }
    }
}
