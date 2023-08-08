using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveUp : EnvBaseAgent
    {
        private IDistanceRewarder upDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
            sensor.AddObservation(controller.env.BridgeZ / controller.env.Width * 2f);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            upDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerX1FromRight);
        }

        protected override void ApplyTaskSpecificReward()
        {
            AddReward(upDistanceRewarder.Reward() * 1f);
        }
    }
}
