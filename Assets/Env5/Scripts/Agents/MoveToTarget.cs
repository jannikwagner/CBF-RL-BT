using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTarget : EnvBaseAgent
    {
        private IDistanceRewarder playerTargetDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 targetPos = controller.env.buttonTrigger.localPosition;
            Vector3 distanceObs = (targetPos - playerPos) / controller.env.width;
            sensor.AddObservation(distanceObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTargetDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.player.localPosition, controller.env.buttonTrigger.localPosition));
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
            AddReward(playerTargetDistanceRewarder.Reward() * rFactor);
        }
    }
}
