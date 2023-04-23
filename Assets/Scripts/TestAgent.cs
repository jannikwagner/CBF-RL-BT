using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TestAgent : Agent
{
    private int StepsPerDecision = 10;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("Fixed update, step count: " + StepCount + ", episode count: " + CompletedEpisodes);

        if (StepCount % StepsPerDecision == 0)
        {
            Debug.Log("Requesting decision");
            RequestDecision();
        }
        else
        {
            Debug.Log("Requesting action");
            RequestAction();
        }

        if (StepCount % 100 == 0)
        {
            this.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Agent initialized");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
        Debug.Log("Heuristic called");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        Debug.Log("Observations collected");
        sensor.AddObservation(0);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        base.WriteDiscreteActionMask(actionMask);
        Debug.Log("Discrete action mask written");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        Debug.Log("Action " + actions + " received");
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        Debug.Log("Episode begin");
    }
}
