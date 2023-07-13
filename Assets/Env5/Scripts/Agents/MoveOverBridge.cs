using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveOverBridge : EnvBaseAgent
    {
        private IDistanceRewarder triggerGoalDistanceRewarder;
        private IDistanceRewarder X3DistanceRewarder;
        // private IDistanceRewarder playerTriggerDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            // Vector3 triggerPos = controller.env.goalTrigger.localPosition;
            // sensor.AddObservation((triggerPos - playerPos) / controller.env.width);
            Vector3 goalPos = controller.env.goal.localPosition;
            sensor.AddObservation((goalPos - playerPos) / controller.env.Width);
            Vector3 targetPos = controller.env.target.localPosition;
            Vector3 distanceToTargetObs = (targetPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTargetObs);  // should not collide
            // sensor.AddObservation((goalPos - triggerPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            triggerGoalDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.env.goalTrigger.localPosition, controller.env.goal.localPosition));

            // playerTriggerDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToGoalTrigger);
            X3DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerX3FromLeft);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Moved Over Bridge! PC: " + PostCondition.Name);
                // AddReward(-1f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            // if (controller.DistanceToTarget() <= 1.0f)
            // {
            //     AddReward(rFactor / maxActions);
            // }

            AddReward(triggerGoalDistanceRewarder.Reward() * 0.5f);
            AddReward(X3DistanceRewarder.Reward() * 0.5f);

            // AddReward(playerTriggerDistanceRewarder.Reward() * rFactor);

        }
    }
}
