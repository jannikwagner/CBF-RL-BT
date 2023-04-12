using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToTarget : BaseAgent
{
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.player.position);
        sensor.AddObservation(controller.env.target.position - controller.player.position);
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        controller.env.Initialize();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        Debug.Log("MoveToTarget.OnActionReceived");
        // is never true!
        if (controller.IsCloseToTarget())
        {
            SetReward(1.0f);
            Debug.Log("Target reached!");
        }
    }
}
