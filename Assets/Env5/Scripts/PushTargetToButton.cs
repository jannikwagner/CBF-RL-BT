using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PushTargetToButton : BaseAgent
{
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.player.position);
        sensor.AddObservation(controller.env.target.position - controller.player.position);
        sensor.AddObservation(controller.env.button.position - controller.player.position);
        sensor.AddObservation(controller.env.button.position - controller.env.target.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        Debug.Log("PushTargetToButton.OnActionReceived");
    }
}
