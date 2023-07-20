using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetToButton : EnvBaseAgent
    {
        private IDistanceRewarder trigger1Button1DistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            Vector3 button1Pos = controller.env.button1.localPosition;
            sensor.AddObservation((button1Pos - playerPos) / controller.env.Width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            trigger1Button1DistanceRewarder = new OnlyImprovingDistanceRewarder(() => Vector3.Distance(controller.env.trigger1.localPosition, controller.env.button1.localPosition));
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Button pressed! PC: " + PostCondition.Name);
                AddReward(-1f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            AddReward(trigger1Button1DistanceRewarder.Reward() * 1f);
        }
    }
}
