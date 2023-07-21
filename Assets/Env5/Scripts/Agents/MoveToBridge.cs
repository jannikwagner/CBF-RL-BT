using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToBridge : EnvBaseAgent
    {
        private IDistanceRewarder playerBridgeDistanceRewarder;
        private IDistanceRewarder playerTrigger1DistancePunisher;
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
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerBridgeDistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.player.localPosition, new Vector3(controller.env.X1, controller.env.ElevatedGroundY, controller.env.bridgeDown.transform.localPosition.z)));

            playerTrigger1DistancePunisher = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger1);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Moved to bridge! PC: " + PostCondition.Name);
                AddReward(-1f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }

            AddReward(playerBridgeDistanceRewarder.Reward() * 1f);
            AddReward(-playerTrigger1DistancePunisher.Reward() * 1f);
        }
    }
}
