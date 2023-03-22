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
    private CBFApplicator[] cbfApplicators;
    private DecisionRequester decisionRequester;


    public override void OnEpisodeBegin()
    {
        // Reset the positions
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        targetTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        enemyTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.gameObject.SetActive(true);

        battery = Random.Range(0.5f, 1f);

        var movementDynamics = new MovementDynamics(this);
        var batteryDynamics = new BatteryDynamics(this);
        enemyCBFApplicator = new CBFApplicator(new MovingBallCBF3D(1.5f), new CombinedDynamics(movementDynamics, enemy));
        wall1CBFApplicator = new CBFApplicator(new WallCBF3D(new Vector3(fieldWidth, 0f, 0f), new Vector3(-1f, 0f, 0f)), movementDynamics);
        wall2CBFApplicator = new CBFApplicator(new WallCBF3D(new Vector3(-fieldWidth, 0f, 0f), new Vector3(1f, 0f, 0f)), movementDynamics);
        wall3CBFApplicator = new CBFApplicator(new WallCBF3D(new Vector3(0f, 0f, fieldWidth), new Vector3(0f, 0f, -1f)), movementDynamics);
        wall4CBFApplicator = new CBFApplicator(new WallCBF3D(new Vector3(0f, 0f, -fieldWidth), new Vector3(0f, 0f, 1f)), movementDynamics);
        enemyCBFApplicatorWide = new CBFApplicator(new MovingBallCBF3D(3f), new CombinedDynamics(movementDynamics, enemy));
        batteryCBFApplicator = new CBFApplicator(new StaticBatteryMarginCBF(batteryTransform.localPosition, 1.5f, batteryConsumption), batteryDynamics, true);
        cbfApplicators = new CBFApplicator[] { enemyCBFApplicator, wall1CBFApplicator, wall2CBFApplicator, wall3CBFApplicator, wall4CBFApplicator, batteryCBFApplicator, enemyCBFApplicatorWide };
        // cbfApplicators = new CBFApplicator[] { };
        decisionRequester = GetComponent<DecisionRequester>();
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

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var numActions = 9;
        bool[] actionMasked = new bool[numActions];
        foreach (var cbfApplicator in cbfApplicators)
        {
            // Debug.Log("CBF: " + cbfApplicator.cbf);
            // Debug.Log(cbfApplicator.evluate());
            bool[] actionMaskedNew = new bool[numActions];
            var allMasked = true;
            for (int i = 0; i < numActions; i++)
            {
                // Debug.Log("Action: " + i);
                var actions = new ActionBuffers(new float[] { }, new int[] { i });
                var okay = cbfApplicator.actionOkayContinuous(actions, decisionRequester.DecisionPeriod);
                bool mask = !okay || actionMasked[i];
                actionMaskedNew[i] = mask;
                allMasked = allMasked && mask;
            }
            if (allMasked)
            {
                Debug.Log("All actions masked! CBF: " + cbfApplicator.cbf);
                break;
            }
            actionMasked = actionMaskedNew;
        }
        for (int i = 0; i < numActions; i++)
        {
            actionMask.SetActionEnabled(0, i, !actionMasked[i]);
        }
        // Debug.Log("Local position: " + transform.localPosition);
        // Debug.Log("State: " + Utility.ArrToVec3(this.currentState()));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 movement = getMovement(actions);

        // Apply the movement
        transform.localPosition += movement * Time.deltaTime;

        AddReward(-0.5f / MaxStep);
        battery -= getBatteryChange(movement) * Time.deltaTime;
        if (battery <= 0)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery empty!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    private float getBatteryChange(Vector3 movement)
    {
        return batteryConsumption * movement.magnitude;
    }

    private Vector3 getMovement(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        var action = discreteActions[0];

        var i = action % 3;
        var j = action / 3;
        var movement = new Vector3(i - 1, 0f, j - 1) * speed;
        return movement;

        // var moveXAction = discreteActions[0];
        // var moveZAction = discreteActions[1];

        // float moveX = moveXAction - 1;
        // float moveZ = moveZAction - 1;

        // var movement = new Vector3(moveX, 0f, moveZ) * speed;
        // return movement;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreateActionsOut = actionsOut.DiscreteActions;

        var i = (int)Input.GetAxisRaw("Horizontal") + 1;
        var j = (int)Input.GetAxisRaw("Vertical") + 1;
        discreateActionsOut[0] = i + 3 * j;
        // Debug.Log(discreateActionsOut[0]);
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
            battery = 1f;
            AddReward(0.1f);
            batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // batteryTransform.gameObject.SetActive(false);
            Debug.Log("Battery collected!");
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
