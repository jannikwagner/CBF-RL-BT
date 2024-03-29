using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTrigger1 : EnvBaseAgent
    {
        private IDistanceRewarder playerTrigger1DistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);

            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);

            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceObs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceObs);

            Vector3 distanceToBridgeObs = (controller.env.BridgeEntranceLeft - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToBridgeObs);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTrigger1DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger1);
        }

        protected override void ApplyTaskSpecificReward()
        {
            AddReward(playerTrigger1DistanceRewarder.Reward() * 1f);
        }
    }
}
