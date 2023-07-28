using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveOverBridge : EnvBaseAgent
    {
        private IDistanceRewarder trigger2Button2DistanceRewarder;
        private IDistanceRewarder X3DistanceRewarder;
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
            trigger2Button2DistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.env.trigger2.localPosition, controller.env.button2.localPosition));

            X3DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerX3FromLeft);
        }

        protected override void ApplyTaskSpecificReward()
        {
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Moved Over Bridge! PC: " + PostCondition.Name);
            }

            AddReward(trigger2Button2DistanceRewarder.Reward() * 0.5f);
            AddReward(X3DistanceRewarder.Reward() * 0.5f);
        }
    }
}
