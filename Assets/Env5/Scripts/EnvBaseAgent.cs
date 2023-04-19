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
        private IEnvActuator actuator;
        void Start()
        {
            actuator = new EnvActuator25();
        }
        public Vector3 GetForce(ActionBuffers actions)
        {
            return actuator.GetForce(actions);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var force = GetForce(actions);
            controller.ApplyForce(force);
            // Debug.Log("BaseAgent.OnActionReceived: " + force);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actuator.Heuristic(actionsOut);
        }

        public override void ResetEnv()
        {
            this.controller.env.Initialize();
        }
        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            // Debug.Log("OnEpisodeBegin");
        }
    }

    public interface IEnvActuator
    {
        Vector3 GetForce(ActionBuffers actions);
        void Heuristic(in ActionBuffers actionsOut);
    }

    public class EnvActuator25 : IEnvActuator
    {

        public Vector3 GetForce(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 5;
            var j = action / 5;
            var force = new Vector3(i - 2, 0f, j - 2) / 2.0f;
            return force;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
            var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
            discreateActionsOut[0] = i + 5 * j;
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class EnvActuator9 : IEnvActuator
    {

        public Vector3 GetForce(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 3;
            var j = action / 3;
            var force = new Vector3(i - 1, 0f, j - 1);
            return force;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal") + 1;
            var j = (int)Input.GetAxisRaw("Vertical") + 1;
            discreateActionsOut[0] = i + 3 * j;
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class EnvActuator5 : IEnvActuator
    {
        public Vector3 GetForce(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            switch (action)
            {
                case 1:
                    return new Vector3(0f, 0f, 1f);
                case 2:
                    return new Vector3(0f, 0f, -1f);
                case 3:
                    return new Vector3(1f, 0f, 0f);
                case 4:
                    return new Vector3(-1f, 0f, 0f);
                case 0:
                default:
                    return new Vector3(0f, 0f, 0f);

            }
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal");
            var j = (int)Input.GetAxisRaw("Vertical");
            if (i == 0 && j == 0)
            {
                discreateActionsOut[0] = 0;
            }
            else if (i == 0)
            {
                discreateActionsOut[0] = j > 0 ? 1 : 2;
            }
            else
            {
                discreateActionsOut[0] = i > 0 ? 3 : 4;
            }
            // Debug.Log(discreateActionsOut[0]);
        }
    }
}
