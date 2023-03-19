using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Env3Agent : Agent
{
    public float speed = 1f;

    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public override void OnEpisodeBegin()
    {
        // Reset the agent's position
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        enemyTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(enemyTransform.localPosition);
        if (StepCount == MaxStep - 1)
        {
            floorMeshRenderer.material = winMaterial;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        var movement = new Vector3(moveX, 0f, moveZ);
        AddReward(1.0f / MaxStep);

        // Apply the movement
        transform.localPosition += movement * speed * Time.deltaTime;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f);
            Debug.Log("Wall hit!");
            Debug.Log("Reward: " + GetCumulativeReward());
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1.0f);
            Debug.Log("Enemy hit!");
            Debug.Log("Reward: " + GetCumulativeReward());
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
