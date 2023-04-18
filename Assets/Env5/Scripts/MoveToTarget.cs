using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTarget : EnvBaseAgent
    {
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 distanceObs = (controller.env.target.localPosition - playerPos) / controller.env.width;
            sensor.AddObservation(distanceObs);
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
