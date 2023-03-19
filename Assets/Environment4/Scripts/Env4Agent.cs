using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Env4Agent : Agent
{
    public float speed = 1f;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform batteryTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public float batteryConsumption = 1f;
    private float battery = 1f;
    public float fieldWidth = 10f;

    public override void OnEpisodeBegin()
    {
        // Reset the positions
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        targetTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        enemyTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.gameObject.SetActive(true);

        battery = Random.Range(0.5f, 1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var factor = 1f;
        sensor.AddObservation(transform.localPosition.x / factor);
        sensor.AddObservation(transform.localPosition.z / factor);
        sensor.AddObservation(targetTransform.localPosition.x / factor);
        sensor.AddObservation(targetTransform.localPosition.z / factor);
        sensor.AddObservation(enemyTransform.localPosition.x / factor);
        sensor.AddObservation(enemyTransform.localPosition.z / factor);
        sensor.AddObservation(batteryTransform.localPosition.x / factor);
        sensor.AddObservation(batteryTransform.localPosition.z / factor);
        sensor.AddObservation(battery);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        var moveXAction = discreteActions[0];
        var moveZAction = discreteActions[1];
        float moveX = moveXAction - 1;
        float moveZ = moveZAction - 1;

        var movement = new Vector3(moveX, 0f, moveZ);

        // Apply the movement
        transform.localPosition += movement * speed * Time.deltaTime;

        AddReward(-0.5f / MaxStep);
        battery -= batteryConsumption * movement.magnitude / MaxStep;
        if (battery <= 0)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery empty!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreateActionsOut = actionsOut.DiscreteActions;

        // var distance = targetTransform.localPosition - transform.localPosition;
        // continuousActionsOut[0] = Mathf.Clamp(distance.x, -1f, 1f);
        // continuousActionsOut[1] = Mathf.Clamp(distance.z, -1f, 1f);

        discreateActionsOut[0] = (int)Input.GetAxisRaw("Horizontal") + 1;
        discreateActionsOut[1] = (int)Input.GetAxisRaw("Vertical") + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
            Debug.Log("Target reached!");
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Target reached!");
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Wall hit!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Enemy hit!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Battery"))
        {
            battery = 1f;
            AddReward(0.1f);
            batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // batteryTransform.gameObject.SetActive(false);
            Debug.Log("Battery collected!");
        }
    }
}
