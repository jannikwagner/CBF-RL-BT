using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Env4Agent : Agent
{
    public Env4Controller controller;
    public Env4ActuatorComponent actuatorComponent;

    public DecisionRequester decisionRequester;
    public bool useCBF = true;

    public CBFApplicator[] cbfApplicators;
    private CBFApplicator enemyCBFApplicator;
    private CBFApplicator enemyCBFApplicatorWide;
    private CBFApplicator wall1CBFApplicator;
    private CBFApplicator wall2CBFApplicator;
    private CBFApplicator wall3CBFApplicator;
    private CBFApplicator wall4CBFApplicator;
    private CBFApplicator batteryCBFApplicator;

    public override void Initialize()
    {
        base.Initialize();
        decisionRequester = GetComponent<DecisionRequester>();
        int steps = decisionRequester.DecisionPeriod;
        float deltaTime = Time.fixedDeltaTime * steps;
        float eta = 1f;
        Func<float, float> alpha = ((float x) => x / deltaTime);

        var movementDynamics = new MovementDynamics(this);
        var batteryDynamics = new BatteryDynamics(this);
        enemyCBFApplicator = new DiscreteCBFApplicator((new MovingBallCBF3D(1.5f)), new CombinedDynamics(movementDynamics, controller.enemy), 0.9f, deltaTime);
        wall1CBFApplicator = new DiscreteCBFApplicator(new SignedSquareCBF(new WallCBF3D(new Vector3(controller.fieldWidth, 0f, 0f), new Vector3(-1f, 0f, 0f))), movementDynamics, eta, deltaTime);
        wall2CBFApplicator = new DiscreteCBFApplicator(new SignedSquareCBF(new WallCBF3D(new Vector3(-controller.fieldWidth, 0f, 0f), new Vector3(1f, 0f, 0f))), movementDynamics, eta, deltaTime);
        wall3CBFApplicator = new DiscreteCBFApplicator(new SignedSquareCBF(new WallCBF3D(new Vector3(0f, 0f, controller.fieldWidth), new Vector3(0f, 0f, -1f))), movementDynamics, eta, deltaTime);
        wall4CBFApplicator = new DiscreteCBFApplicator(new SignedSquareCBF(new WallCBF3D(new Vector3(0f, 0f, -controller.fieldWidth), new Vector3(0f, 0f, 1f))), movementDynamics, eta, deltaTime);
        // enemyCBFApplicatorWide = new DiscreteCBFApplicator(new MovingBallCBF3D(3f), new CombinedDynamics(movementDynamics, enemy), eta, deltaTime);
        // batteryCBFApplicator = new DiscreteCBFApplicator(new StaticBatteryMarginCBF(controller.batteryTransform.localPosition, 1.5f, batteryConsumption), batteryDynamics, eta, deltaTime);
        cbfApplicators = new CBFApplicator[] { enemyCBFApplicator, wall1CBFApplicator, wall2CBFApplicator, wall3CBFApplicator, wall4CBFApplicator, };
        if (!useCBF) cbfApplicators = new CBFApplicator[] { };
    }

    public override void OnEpisodeBegin()
    {
        controller.Initialize();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var factor = 2f / controller.fieldWidth;
        Vector3 localPosition = controller.playerTransform.localPosition;

        Vector3 localPos = 2 * factor * localPosition;
        Vector3 targetPos = (controller.targetTransform.localPosition - localPosition) * factor;
        Vector3 enemyPos = (controller.enemyTransform.localPosition - localPosition) * factor;
        Vector3 batteryPos = (controller.batteryTransform.localPosition - localPosition) * factor;
        sensor.AddObservation(localPos.x);
        sensor.AddObservation(localPos.z);
        sensor.AddObservation(targetPos.x);
        sensor.AddObservation(targetPos.z);
        sensor.AddObservation(enemyPos.x);
        sensor.AddObservation(enemyPos.z);
        sensor.AddObservation(batteryPos.x);
        sensor.AddObservation(batteryPos.z);
        sensor.AddObservation(controller.battery);
    }

    public void Move(Vector3 movement)
    {
        controller.Move(movement);

        if (controller.battery <= 0)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery empty!");
            controller.floorMeshRenderer.material = controller.loseMaterial;
            EndEpisode();
        }
        if (StepCount >= MaxStep - 1)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Max steps reached!");
            controller.floorMeshRenderer.material = controller.loseMaterial;
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + controller.battery + " | Target reached!");
            controller.floorMeshRenderer.material = controller.winMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + controller.battery + " | Wall hit!");
            controller.floorMeshRenderer.material = controller.loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + controller.battery + " | Enemy hit!");
            controller.floorMeshRenderer.material = controller.loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Battery"))
        {
            AddReward(0.1f);
            Debug.Log("Battery collected with Battery: " + controller.battery);
            controller.battery = 1f;
            controller.batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // controller.batteryTransform.gameObject.SetActive(false);
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
            return Utility.vec3ToArr(agent.actuatorComponent.GetMovement(action, agent.controller.speed));
        }

        public float[] currentState()
        {
            return Utility.vec3ToArr(agent.controller.playerTransform.localPosition);
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
            var movement = agent.actuatorComponent.GetMovement(action, agent.controller.speed);
            var batteryChange = agent.controller.getBatteryChange(movement);
            return Utility.combineArrs(Utility.vec3ToArr(movement), new float[] { batteryChange });
        }

        public float[] currentState()
        {
            return Utility.combineArrs(Utility.vec3ToArr(agent.controller.playerTransform.localPosition), new float[] { agent.controller.battery });
        }
    }
}
