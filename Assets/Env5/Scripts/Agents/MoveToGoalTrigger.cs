using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToGoalTrigger : EnvBaseAgent
    {
        private IDistanceRewarder playerTriggerDistanceRewarder;
        private IDistanceRewarder playerTargetDistancePunisher;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 goalTriggerPos = controller.env.goalTrigger.localPosition;
            Vector3 distanceToGoalTriggerObs = (goalTriggerPos - playerPos) / controller.env.width;
            sensor.AddObservation(distanceToGoalTriggerObs);
            Vector3 targetPos = controller.env.target.localPosition;
            Vector3 distanceToTargetObs = (targetPos - playerPos) / controller.env.width;
            sensor.AddObservation(distanceToTargetObs);  // should not collide
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTriggerDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToGoalTrigger);
            playerTargetDistancePunisher = new OnlyImprovingDistanceRewarder(controller.DistanceToTarget);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            // Debug.Log("MoveToTarget.OnActionReceived");
            if (PostCondition != null && PostCondition())
            {
                Debug.Log("GoalTrigger reached!");

                float velocityPunishment = -0.1f * controller.rb.velocity.magnitude / controller.maxSpeed;
                // Debug.Log("velocityPunishment: " + velocityPunishment);
                AddReward(velocityPunishment);
            }
            AddReward(playerTriggerDistanceRewarder.Reward() * 1f);
            AddReward(-playerTargetDistancePunisher.Reward() * 1f);
        }
    }
}
