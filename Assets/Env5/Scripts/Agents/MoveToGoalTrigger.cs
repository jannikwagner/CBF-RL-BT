using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToGoalTrigger : EnvBaseAgent
    {
        private IDistanceRewarder playerTriggerDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 goalTriggerPos = controller.env.goalTrigger.localPosition;
            Vector3 distanceObs = (goalTriggerPos - playerPos) / controller.env.width;
            sensor.AddObservation(distanceObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTriggerDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToGoalTrigger);
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
        }
    }
}
