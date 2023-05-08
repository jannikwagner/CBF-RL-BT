using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetToButton : EnvBaseAgent
    {
        // private IDistanceRewarder playerTargetDistanceRewarder;
        private IDistanceRewarder targetButtonDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            // Vector3 targetPos = controller.env.target.localPosition;
            // sensor.AddObservation((targetPos - playerPos) / controller.env.width);
            Vector3 buttonPos = controller.env.button.localPosition;
            sensor.AddObservation((buttonPos - playerPos) / controller.env.Width);
            // sensor.AddObservation((buttonPos - targetPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            targetButtonDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.env.target.localPosition, controller.env.button.localPosition));

            // playerTargetDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTarget);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition())
            {
                Debug.Log("Button pressed!");
                AddReward(-1f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            // if (controller.DistanceToTarget() <= 1.0f)
            // {
            //     AddReward(rFactor / maxActions);
            // }
            AddReward(targetButtonDistanceRewarder.Reward() * 1f);

            // AddReward(playerTargetDistanceRewarder.Reward() * rFactor);
        }
    }
}
