using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToTarget : BaseAgent
{
    public Transform target;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.player.position);
        sensor.AddObservation(target.position - controller.player.position);
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        controller.envController.Initialize();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        if (controller.isCloseToTarget())
        {
            SetReward(1.0f);
            Debug.Log("Target reached!");
        }
    }
}
