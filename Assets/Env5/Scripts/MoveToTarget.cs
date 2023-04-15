using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTarget : EnvBaseAgent
    {
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(controller.player.position);
            sensor.AddObservation(controller.env.target.position - controller.player.position);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            // Debug.Log("MoveToTarget.OnActionReceived");
            if (controller.IsCloseToTarget())
            {
                Debug.Log("Target reached!");
            }
        }
    }
}
