using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Env4Agent : Agent
{
    public float speed = 1f;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform batteryTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public float batteryConsumption = 0.00f;
    public float battery = 1f;
    public float fieldWidth = 9f;
    public EnemyBehavior4 enemy;
    private CBFApplicator enemyCBFApplicator;
    private CBFApplicator enemyCBFApplicatorWide;
    private CBFApplicator wall1CBFApplicator;
    private CBFApplicator wall2CBFApplicator;
    private CBFApplicator wall3CBFApplicator;
    private CBFApplicator wall4CBFApplicator;
    private CBFApplicator batteryCBFApplicator;
    public CBFApplicator[] cbfApplicators;
    public DecisionRequester decisionRequester;
    public bool useCBF = true;

    public void Start()
    {
        decisionRequester = GetComponent<DecisionRequester>();
        int steps = decisionRequester.DecisionPeriod;
        float deltaTime = Time.fixedDeltaTime * steps;
        float eta = 0.5f;
        Func<float, float> alpha = ((float x) => x / deltaTime);

        var movementDynamics = new MovementDynamics(this);
        var batteryDynamics = new BatteryDynamics(this);
        enemyCBFApplicator = new DiscreteCBFApplicator(new MovingBallCBF3D(1.5f), new CombinedDynamics(movementDynamics, enemy), eta, deltaTime);
        wall1CBFApplicator = new DiscreteCBFApplicator(new WallCBF3D(new Vector3(fieldWidth, 0f, 0f), new Vector3(-1f, 0f, 0f)), movementDynamics, eta, deltaTime);
        wall2CBFApplicator = new DiscreteCBFApplicator(new WallCBF3D(new Vector3(-fieldWidth, 0f, 0f), new Vector3(1f, 0f, 0f)), movementDynamics, eta, deltaTime);
        wall3CBFApplicator = new DiscreteCBFApplicator(new WallCBF3D(new Vector3(0f, 0f, fieldWidth), new Vector3(0f, 0f, -1f)), movementDynamics, eta, deltaTime);
        wall4CBFApplicator = new DiscreteCBFApplicator(new WallCBF3D(new Vector3(0f, 0f, -fieldWidth), new Vector3(0f, 0f, 1f)), movementDynamics, eta, deltaTime);
        enemyCBFApplicatorWide = new DiscreteCBFApplicator(new MovingBallCBF3D(3f), new CombinedDynamics(movementDynamics, enemy), eta, deltaTime);
        batteryCBFApplicator = new DiscreteCBFApplicator(new StaticBatteryMarginCBF(batteryTransform.localPosition, 1.5f, batteryConsumption), batteryDynamics, eta, deltaTime);
        cbfApplicators = new CBFApplicator[] { enemyCBFApplicator, wall1CBFApplicator, wall2CBFApplicator, wall3CBFApplicator, wall4CBFApplicator, batteryCBFApplicator, };
        // cbfApplicators = new CBFApplicator[] { };
    }


    public override void OnEpisodeBegin()
    {
        // Reset the positions
        var xMin = -fieldWidth + 1f;
        var xMax = fieldWidth - 1f;
        var zMin = -fieldWidth + 1f;
        var zMax = fieldWidth - 1f;
        var yMin = 0f;
        var yMax = 0f;
        var minDistance = 1.1f;
        transform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { });
        targetTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition });
        enemyTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition, targetTransform.localPosition });
        batteryTransform.localPosition = Utility.SamplePosition(xMin, xMax, zMin, zMax, yMin, yMax, minDistance, new Vector3[] { transform.localPosition, targetTransform.localPosition, enemyTransform.localPosition });
        batteryTransform.gameObject.SetActive(true);

        battery = Random.Range(0.1f, 1f);
        enemy.speed = Random.Range(0f, 2f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var factor = 2f / fieldWidth;
        Vector3 localPosition = transform.localPosition * 2 * factor;
        Vector3 targetPos = (targetTransform.localPosition - localPosition) * factor;
        Vector3 enemyPos = (enemyTransform.localPosition - localPosition) * factor;
        Vector3 batteryPos = (batteryTransform.localPosition - localPosition) * factor;
        sensor.AddObservation(localPosition.x);
        sensor.AddObservation(localPosition.z);
        sensor.AddObservation(targetPos.x);
        sensor.AddObservation(targetPos.z);
        sensor.AddObservation(enemyPos.x);
        sensor.AddObservation(enemyPos.z);
        sensor.AddObservation(batteryPos.x);
        sensor.AddObservation(batteryPos.z);
        sensor.AddObservation(battery);
    }

    public void Move(Vector3 movement)
    {
        transform.localPosition += movement * Time.deltaTime;

        // AddReward(-0.5f / MaxStep);
        battery -= getBatteryChange(movement) * Time.deltaTime;
        if (battery <= 0)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery empty!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        if (StepCount >= MaxStep - 1)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Max steps reached!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
    private float getBatteryChange(Vector3 movement)
    {
        return batteryConsumption * movement.magnitude;
    }

    public Vector3 getMovement(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        var action = discreteActions[0];

        var i = action % 5;
        var j = action / 5;
        var movement = new Vector3(i - 2, 0f, j - 2) * speed / 2.0f;
        return movement;

        // var moveXAction = discreteActions[0];
        // var moveZAction = discreteActions[1];

        // float moveX = moveXAction - 1;
        // float moveZ = moveZAction - 1;

        // var movement = new Vector3(moveX, 0f, moveZ) * speed;
        // return movement;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
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
            AddReward(0.1f);
            Debug.Log("Battery collected with Battery: " + battery);
            battery = 1f;
            batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // batteryTransform.gameObject.SetActive(false);
        }
    }

    public class MovementDynamics : IControlledDynamics
    {
        private Env4Agent agent;
        public MovementDynamics(Env4Agent agent)
        {
            this.agent = agent;
        }

        public float[] ControlledDynamics(ActionBuffers action)
        {
            return Utility.vec3ToArr(agent.getMovement(action));
        }

        public float[] currentState()
        {
            return Utility.vec3ToArr(agent.transform.localPosition);
        }
    }

    public class BatteryDynamics : IControlledDynamics
    {
        private Env4Agent agent;
        public BatteryDynamics(Env4Agent agent)
        {
            this.agent = agent;
        }

        public float[] ControlledDynamics(ActionBuffers action)
        {
            var movement = agent.getMovement(action);
            var batteryChange = agent.getBatteryChange(movement);
            return Utility.combineArrs(Utility.vec3ToArr(movement), new float[] { batteryChange });
        }

        public float[] currentState()
        {
            return Utility.combineArrs(Utility.vec3ToArr(agent.transform.localPosition), new float[] { agent.battery });
        }
    }

}
