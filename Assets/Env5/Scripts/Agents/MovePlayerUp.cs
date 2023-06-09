using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MovePlayerUp : EnvBaseAgent
    {
        private IDistanceRewarder targetUpDistanceRewarder;
        // private IDistanceRewarder playerTargetDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            // Vector3 targetPos = controller.env.target.localPosition;
            // sensor.AddObservation((targetPos - playerPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            targetUpDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerBeforeX1);

            // playerTargetDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTarget);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Target up! PC: " + PostCondition.Name);
                AddReward(-0.0f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            // if (controller.DistanceToTarget() <= 1.0f)
            // {
            //     AddReward(rFactor / maxActions);
            // }
            AddReward(targetUpDistanceRewarder.Reward() * 1f);

            // AddReward(playerTargetDistanceRewarder.Reward() * rFactor);
        }
    }
}
