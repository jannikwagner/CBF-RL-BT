using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public interface IEnvActuator
    {
        int NumActions { get; }
        Vector3 GetAcceleration(ActionBuffers actions);
        void Heuristic(in ActionBuffers actionsOut);
    }

    public class EnvActuatorGrid5x5Norm : IEnvActuator
    {
        public int NumActions => 25;
        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 5;
            var j = action / 5;
            var x = i - 2;
            var z = j - 2;

            var acceleration = new Vector3(x, 0f, z) / 2.0f;
            // full speed diagonal / half diagonal
            if (acceleration.magnitude > 1f)
            {
                acceleration = acceleration.normalized;
            }
            // half speed diagonal
            else if (Mathf.Abs(x) == 1 && Mathf.Abs(z) == 1)
            {
                acceleration = acceleration.normalized * 0.5f;
            }
            return acceleration;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
            var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
            discreateActionsOut[0] = i + 5 * j;
        }
    }

    public class EnvActuatorGrid5x5 : IEnvActuator
    {
        public int NumActions => 25;
        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 5;
            var j = action / 5;
            var acceleration = new Vector3(i - 2, 0f, j - 2) / 2.0f;
            return acceleration;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
            var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
            discreateActionsOut[0] = i + 5 * j;
        }
    }
    public class EnvActuatorGrid3x3Norm : IEnvActuator
    {
        public int NumActions => 9;

        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 3;
            var j = action / 3;
            var acceleration = new Vector3(i - 1, 0f, j - 1);
            // diagonal
            if (acceleration.magnitude > 1f)
            {
                acceleration = acceleration.normalized;
            }
            return acceleration;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal") + 1;
            var j = (int)Input.GetAxisRaw("Vertical") + 1;
            discreateActionsOut[0] = i + 3 * j;
        }
    }
    public class EnvActuatorGrid3x3 : IEnvActuator
    {
        public int NumActions => 9;

        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 3;
            var j = action / 3;
            var acceleration = new Vector3(i - 1, 0f, j - 1);
            return acceleration;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal") + 1;
            var j = (int)Input.GetAxisRaw("Vertical") + 1;
            discreateActionsOut[0] = i + 3 * j;
        }
    }
    public class EnvActuator5 : IEnvActuator
    {
        public int NumActions => 5;

        public Vector3 GetAcceleration(ActionBuffers actions)
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
    public class EnvActuatorCircular : IEnvActuator
    {
        public int directions;
        public int strengths;
        public int NumActions => directions * strengths + 1;

        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            if (action == 0)
            {
                return new Vector3(0f, 0f, 0f);
            }
            else
            {
                var i = (action - 1) % directions;
                var j = (action - 1) / directions;
                var force = new Vector3(i - directions / 2, 0f, j - strengths / 2);
                return force;
            }
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal");
            var j = (int)Input.GetAxisRaw("Vertical");
        }
    }
}