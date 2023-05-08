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
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 targetPos = controller.env.target.localPosition;
            Vector3 distanceObs = (targetPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTargetDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTarget);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            // Debug.Log("MoveToTarget.OnActionReceived");
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Target reached! PC: " + PostCondition.Name);

                float velocityPunishment = -0.1f * controller.rb.velocity.magnitude / controller.maxSpeed;
                // Debug.Log("velocityPunishment: " + velocityPunishment);
                AddReward(velocityPunishment);
            }
            AddReward(playerTargetDistanceRewarder.Reward() * 1f);
        }
    }
}
