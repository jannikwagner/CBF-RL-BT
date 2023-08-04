using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToBridge : EnvBaseAgent
    {
        private IDistanceRewarder playerBridgeDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            Vector3 button2Pos = controller.env.button2.localPosition;
            sensor.AddObservation((button2Pos - playerPos) / controller.env.Width);
            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceToTrigger1Obs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTrigger1Obs);  // should not collide
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
            sensor.AddObservation(controller.env.BridgeZ / controller.env.Width);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            playerBridgeDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.player.localPosition, new Vector3(controller.env.X1, controller.env.ElevatedGroundY, controller.env.bridgeDown.transform.localPosition.z)));
        }

        protected override void ApplyTaskSpecificReward()
        {
            AddReward(playerBridgeDistanceRewarder.Reward() * 1f);
        }
    }
}
