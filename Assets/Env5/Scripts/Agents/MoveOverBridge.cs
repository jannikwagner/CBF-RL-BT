using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveOverBridge : EnvBaseAgent
    {
        private IDistanceRewarder X3DistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);

            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);

            Vector3 distanceToBridgeObs = (controller.env.BridgeEntranceLeft - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToBridgeObs);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            X3DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerX3FromLeft);
        }

        protected override void ApplyTaskSpecificReward()
        {
            AddReward(X3DistanceRewarder.Reward() * 1f);
        }
    }
}
