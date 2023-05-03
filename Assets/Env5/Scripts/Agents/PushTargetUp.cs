using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetUp : EnvBaseAgent
    {
        private IDistanceRewarder targetUpDistanceRewarder;
        private IDistanceRewarder playerTargetDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.width * 2f);
            Vector3 targetPos = controller.env.target.localPosition;
            sensor.AddObservation((targetPos - playerPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            targetUpDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistanceTargetUp);

            playerTargetDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTarget);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            const float rFactor = 0.1f;

            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition())
            {
                Debug.Log("Target up!");
                AddReward(-rFactor * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            // if (controller.DistanceToTarget() <= 1.0f)
            // {
            //     AddReward(rFactor / maxActions);
            // }
            AddReward(targetUpDistanceRewarder.Reward() * rFactor);

            AddReward(playerTargetDistanceRewarder.Reward() * rFactor);
        }
    }
}
