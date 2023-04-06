using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class Env4Controller : MonoBehaviour
{
    public float speed = 1f;

    [SerializeField] public Transform targetTransform;
    [SerializeField] public Transform playerTransform;
    [SerializeField] public Transform enemyTransform;
    [SerializeField] public Transform batteryTransform;
    [SerializeField] public Material winMaterial;
    [SerializeField] public Material loseMaterial;
    [SerializeField] public MeshRenderer floorMeshRenderer;

    public float batteryConsumption = 0.00f;
    public float battery = 1f;
    public float fieldWidth = 9f;

    public EnemyBehavior4 enemy;

    public void Initialize()
    {
        // Reset the positions
        var xMin = -fieldWidth + 1f;
        var xMax = fieldWidth - 1f;
        var zMin = -fieldWidth + 1f;
        var zMax = fieldWidth - 1f;
        var yMin = 0f;
        var yMax = 0f;
        var minDistance = 1.1f;
        playerTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { });
        targetTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition });
        enemyTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition, targetTransform.localPosition });
        batteryTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition, targetTransform.localPosition, enemyTransform.localPosition });
        batteryTransform.gameObject.SetActive(true);

        battery = Random.Range(0.1f, 1f);
        enemy.speed = Random.Range(enemy.minSpeed, enemy.maxSpeed);
    }

    public void Move(Vector3 movement)
    {
        playerTransform.localPosition += movement * Time.fixedDeltaTime;

        // AddReward(-0.5f / MaxStep);
        battery -= getBatteryChange(movement) * Time.fixedDeltaTime;
    }

    public float getBatteryChange(Vector3 movement)
    {
        return batteryConsumption * movement.magnitude;
    }


}
