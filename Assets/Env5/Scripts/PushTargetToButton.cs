using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetToButton : EnvBaseAgent
    {
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(controller.player.position);
            sensor.AddObservation(controller.env.target.position - controller.player.position);
            sensor.AddObservation(controller.env.button.position - controller.player.position);
            sensor.AddObservation(controller.env.button.position - controller.env.target.position);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            // Debug.Log("PushTargetToButton.OnActionReceived");
            if (controller.DistanceToTarget() < 1.0f)
            {
                AddReward(0.2f / 1000f);
            }
        }
    }
}