using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public abstract class EnvBaseAgent : BaseAgent
    {
        public override void CollectObservations(VectorSensor sensor)  // currently not used but overridden
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceToTrigger1Obs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTrigger1Obs);
            Vector3 Trigger2Pos = controller.env.trigger2.localPosition;
            Vector3 distanceToTrigger2Obs = (Trigger2Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTrigger2Obs);
            Vector3 button1Pos = controller.env.button1.localPosition;
            Vector3 distanceToButton1Obs = (button1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToButton1Obs);
            Vector3 button2Pos = controller.env.button2.localPosition;
            Vector3 distanceTobutton2Obs = (button2Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceTobutton2Obs);
            sensor.AddObservation(controller.env.BridgeZ / controller.env.Width);
        }

        private const float MAX_VELOCITY_PUNISHMENT = -0f;
        private const float ALLOWED_VELOCITY_FRACTION = 0.3f;
        public PlayerController controller;
        private IEnvActuator actuator;
        public override int NumActions => actuator.NumActions;

        void Start()
        {
            actuator = new EnvActuatorGrid5x5Norm();
        }
        public Vector3 GetAcceleration(ActionBuffers actions)
        {
            return actuator.GetAcceleration(actions) * controller.MaxAcc;
        }

        protected override void ApplyAction(ActionBuffers actions)
        {
            var acceleration = GetAcceleration(actions);
            controller.ApplyAcceleration(acceleration);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actuator.Heuristic(actionsOut);
            if (useCBF && CBFApplicators != null)
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
            this.controller.env.Reset();
        }
        public override void ResetEnvGlobal()
        {
            this.controller.env.Reset();
        }

        protected override void OnPCReached(Condition pc)
        {
            base.OnPCReached(pc);
            ApplyVelocityPunishment();
        }

        protected void ApplyVelocityPunishment()
        {
            var velocityFraction = controller.rb.velocity.magnitude / controller.maxSpeed;
            if (velocityFraction > ALLOWED_VELOCITY_FRACTION)
            {
                AddReward(MAX_VELOCITY_PUNISHMENT * (velocityFraction - ALLOWED_VELOCITY_FRACTION) / (1f - ALLOWED_VELOCITY_FRACTION));
            }
        }

    }
}
