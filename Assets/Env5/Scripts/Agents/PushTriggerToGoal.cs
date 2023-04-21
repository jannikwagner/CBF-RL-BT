using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTriggerToGoal : EnvBaseAgent
    {
        private IDistanceRewarder triggerGoalDistanceRewarder;
        private IDistanceRewarder playerTriggerDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.width * 2f);
            Vector3 triggerPos = controller.env.goalTrigger.localPosition;
            sensor.AddObservation((triggerPos - playerPos) / controller.env.width);
            Vector3 goalPos = controller.env.goal.localPosition;
            sensor.AddObservation((goalPos - playerPos) / controller.env.width);
            sensor.AddObservation((goalPos - triggerPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            triggerGoalDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.env.goalTrigger.localPosition, controller.env.goal.localPosition));

            playerTriggerDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.player.localPosition, controller.env.buttonTrigger.localPosition));

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            const float rFactor = 0.1f;

            base.OnActionReceived(actions);
            if (controller.env.ButtonPressed())
            {
                // Debug.Log("Button pressed!");
                AddReward(-rFactor * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            if (controller.DistanceToTarget() < 1.0f)
            {
                AddReward(rFactor / 1000f);
            }

            AddReward(triggerGoalDistanceRewarder.Reward() * rFactor * 3);

            AddReward(playerTriggerDistanceRewarder.Reward() * rFactor);
        }
    }
}
