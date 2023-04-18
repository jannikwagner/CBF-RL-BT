using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetToButton : EnvBaseAgent
    {
        private float startDistanceTargetButton;
        private float lastDistanceTargetButton;
        private float startDistancePlayerTarget;
        private float lastDistancePlayerTarget;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.width * 2f);
            Vector3 targetPos = controller.env.target.localPosition;
            sensor.AddObservation((targetPos - playerPos) / controller.env.width);
            Vector3 buttonPos = controller.env.button.localPosition;
            sensor.AddObservation((buttonPos - playerPos) / controller.env.width);
            sensor.AddObservation((buttonPos - targetPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            startDistanceTargetButton = Vector3.Distance(controller.env.target.localPosition, controller.env.button.localPosition);
            lastDistanceTargetButton = startDistanceTargetButton;

            startDistancePlayerTarget = Vector3.Distance(controller.player.localPosition, controller.env.target.localPosition);
            lastDistancePlayerTarget = startDistancePlayerTarget;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            const float rFactor = 0.1f;

            base.OnActionReceived(actions);
            if (controller.env.ButtonPressed())
            {
                Debug.Log("Button pressed!");
                AddReward(-rFactor * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            // Debug.Log("PushTargetToButton.OnActionReceived");
            if (controller.DistanceToTarget() < 1.0f)
            {
                AddReward(rFactor / 1000f);
            }
            float distanceTargetButton = Vector3.Distance(controller.env.target.localPosition, controller.env.button.localPosition);
            AddReward((lastDistanceTargetButton - distanceTargetButton) / startDistanceTargetButton * rFactor);
            lastDistanceTargetButton = distanceTargetButton;

            float distancePlayerTarget = Vector3.Distance(controller.player.localPosition, controller.env.target.localPosition);
            AddReward((lastDistancePlayerTarget - distancePlayerTarget) / startDistancePlayerTarget * rFactor);
            lastDistancePlayerTarget = distancePlayerTarget;
        }
    }
}
