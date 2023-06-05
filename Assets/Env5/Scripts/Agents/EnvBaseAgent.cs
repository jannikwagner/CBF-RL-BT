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
        public override void CollectObservations(VectorSensor sensor)  // currently not used but overridden
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
            Vector3 targetPos = controller.env.target.localPosition;
            Vector3 distanceToTargetObs = (targetPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTargetObs);
            Vector3 goalTriggerPos = controller.env.goalTrigger.localPosition;
            Vector3 distanceToGoalTriggerObs = (goalTriggerPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToGoalTriggerObs);
            Vector3 buttonPos = controller.env.button.localPosition;
            Vector3 distanceToButtonObs = (buttonPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToButtonObs);
            Vector3 goalPos = controller.env.goal.localPosition;
            Vector3 distanceToGoalObs = (goalPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToGoalObs);
        }

        public PlayerController controller;
        private IEnvActuator actuator;
        public override int NumActions => actuator.NumActions;

        void Start()
        {
            actuator = new EnvActuator25();
        }
        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            return actuator.GetAcceleration(actions) * controller.MaxAcc;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var acceleration = GetAcceleration(actions);
            controller.ApplyAcceleration(acceleration);
            base.OnActionReceived(actions);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actuator.Heuristic(actionsOut);
            if (CBFApplicators != null)
            {
                var allowedActions = CBFDiscreteInvalidActionMasker.AllowedActions(CBFApplicators, NumActions);
                var discreateActionsOut = actionsOut.DiscreteActions;
                var chosenAction = discreateActionsOut[0];
                if (!allowedActions.Contains(chosenAction))
                {
                    int i = Random.Range(0, allowedActions.Count);
                    discreateActionsOut[0] = allowedActions[i];
                }
            }
        }

        public override void ResetEnvLocal()
        {
            this.controller.env.Initialize();
        }
        public override void ResetEnvGlobal()
        {
            this.controller.env.Initialize();
        }

        protected override void OnACCViolation()
        {
            // Debug.Log("OnACCViolation" + controller.player.localPosition + controller.env.PlayerUp() + controller.env.DistancePlayerUp());
        }
    }

    public interface IEnvActuator
    {
        int NumActions { get; }
        Vector3 GetAcceleration(ActionBuffers actions);
        void Heuristic(in ActionBuffers actionsOut);
    }

    public class EnvActuator25 : IEnvActuator
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
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class EnvActuator9 : IEnvActuator
    {
        public int NumActions => 9;

        public Vector3 GetAcceleration(ActionBuffers actions)
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
    public class EnvCircularActuator : IEnvActuator
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
    public class PlayerPosVelDynamics : IDynamicsProvider
    {
        EnvBaseAgent agent;
        public PlayerPosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var velocity = agent.controller.rb.velocity;
            var acceleration = agent.GetAcceleration(action);
            // apply acceleration before velocity! needed to be safe because of discretization of continuous system
            // note: applying all steps accels at once is safer than actually necessary, but also simpler.
            float deltaTime = Time.fixedDeltaTime * agent.ActionsPerDecision;
            velocity = velocity + acceleration * deltaTime;
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition;
            var velocity = agent.controller.rb.velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
    public class PlayerTargetPosVelDynamics : IDynamicsProvider
    {
        // relative position and velocity of player to target
        EnvBaseAgent agent;
        public PlayerTargetPosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var targetVelocity = agent.controller.env.target.GetComponentInParent<Rigidbody>().velocity;
            var velocity = agent.controller.rb.velocity - targetVelocity;
            var acceleration = agent.GetAcceleration(action) + 0.5f * targetVelocity;
            float deltaTime = Time.fixedDeltaTime * agent.ActionsPerDecision;
            velocity = velocity + acceleration * deltaTime;
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition - agent.controller.env.target.localPosition;
            var velocity = agent.controller.rb.velocity - agent.controller.env.target.GetComponentInParent<Rigidbody>().velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
}
