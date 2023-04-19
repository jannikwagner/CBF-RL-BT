using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PushTargetUp : EnvBaseAgent
    {
        private float startDistanceTargetUp;
        private float lastDistanceTargetUp;
        private float startDistancePlayerTarget;
        private float lastDistancePlayerTarget;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.width * 2f);
            Vector3 targetPos = controller.env.target.localPosition;
            sensor.AddObservation((targetPos - playerPos) / controller.env.width);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            startDistanceTargetUp = controller.env.DistanceTargetUp();
            lastDistanceTargetUp = startDistanceTargetUp;

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
            float distanceTargetUp = controller.env.DistanceTargetUp();
            AddReward((lastDistanceTargetUp - distanceTargetUp) / startDistanceTargetUp * rFactor * 3);
            lastDistanceTargetUp = distanceTargetUp;
        }
    }
}
