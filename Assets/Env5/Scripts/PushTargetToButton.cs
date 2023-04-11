using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PushTargetToButton : BaseAgent
{
    public Transform target;
    public Transform button;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.player.position);
        sensor.AddObservation(target.position - controller.player.position);
        sensor.AddObservation(button.position - controller.player.position);
        sensor.AddObservation(button.position - target.position);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
