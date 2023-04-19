using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTarget : EnvBaseAgent
    {
        private float startDistancePlayerTarget;
        private float lastDistancePlayerTarget;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 distanceObs = (controller.env.target.localPosition - playerPos) / controller.env.width;
            sensor.AddObservation(distanceObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            startDistancePlayerTarget = Vector3.Distance(controller.player.localPosition, controller.env.target.localPosition);
            lastDistancePlayerTarget = startDistancePlayerTarget;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            const float rFactor = 0.1f;

            base.OnActionReceived(actions);
            // Debug.Log("MoveToTarget.OnActionReceived");
            if (controller.IsCloseToTarget())
            {
                Debug.Log("Target reached!");

                float velocityPunishment = -rFactor * controller.rb.velocity.magnitude / controller.maxSpeed;
                // Debug.Log("velocityPunishment: " + velocityPunishment);
                AddReward(velocityPunishment);
            }

            float distancePlayerTarget = Vector3.Distance(controller.player.localPosition, controller.env.target.localPosition);
            AddReward((lastDistancePlayerTarget - distancePlayerTarget) / startDistancePlayerTarget * rFactor);
            lastDistancePlayerTarget = distancePlayerTarget;
        }
    }
}
