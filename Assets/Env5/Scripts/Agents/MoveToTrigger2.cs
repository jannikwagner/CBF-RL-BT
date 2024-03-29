using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTrigger2 : EnvBaseAgent
    {
        private IDistanceRewarder playerTrigger2DistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);

            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);

            Vector3 trigger2Pos = controller.env.trigger2.localPosition;
            Vector3 distanceTotrigger2Obs = (trigger2Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceTotrigger2Obs);

            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceToTrigger1Obs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTrigger1Obs);  // should not collide

            Vector3 distanceToBridgeObs = (controller.env.BridgeEntranceLeft - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToBridgeObs);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTrigger2DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger2);
        }

        protected override void ApplyTaskSpecificReward()
        {
            AddReward(playerTrigger2DistanceRewarder.Reward() * 1f);
        }
    }
}
