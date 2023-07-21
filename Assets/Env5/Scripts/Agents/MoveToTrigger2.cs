using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTrigger2 : EnvBaseAgent
    {
        private IDistanceRewarder playerTrigger2DistanceRewarder;
        private IDistanceRewarder playerTrigger1DistancePunisher;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 trigger2Pos = controller.env.trigger2.localPosition;
            Vector3 distanceTotrigger2Obs = (trigger2Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceTotrigger2Obs);
            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceToTrigger1Obs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTrigger1Obs);  // should not collide
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTrigger2DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger2);
            playerTrigger1DistancePunisher = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger1);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Trigger2 reached! PC: " + PostCondition.Name);

                float velocityPunishment = -0.1f * controller.rb.velocity.magnitude / controller.maxSpeed;
                AddReward(velocityPunishment);
            }
            AddReward(playerTrigger2DistanceRewarder.Reward() * 1f);
            AddReward(-playerTrigger1DistancePunisher.Reward() * 1f);
        }
    }
}
