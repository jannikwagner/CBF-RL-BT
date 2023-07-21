using UnityEngine;
using Unity.MLAgents;
using BTTest;
using System.Collections.Generic;

namespace Env5
{
    public class MyBTTest : MonoBehaviour, ILogDataProvider
    {
        [SerializeField]
        private BT _tree;
        public IAgentSwitcher agentSwitcher;
        public MoveUp moveUp;
        public MoveToBridge moveToBridge;
        public MoveOverBridge moveOverBridge;
        public MoveToTrigger1 moveToTrigger1;
        public MoveToButton1 moveToButton1;
        public MoveToTrigger2 moveToTrigger2;
        public MoveToButton2 moveToButton2;
        public PlayerController controller;
        public bool useCBF;
        private int maxSteps = 10000;
        private int stepCount;
        private int compositeEpisodeCount;
        private IEvaluationManager evaluationManager;
        List<Condition> conditions;
        public bool evaluationActive;

        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        public int Step => stepCount;
        public int Episode => compositeEpisodeCount;
        public BaseAgent Agent => agentSwitcher.Agent;
        public Action Action => _tree.CurrentAction;

        private void Start()
        {
            Debug.Log("MyBTTest Start");
            var agents = new EnvBaseAgent[] { moveToTrigger1, moveToButton1, moveUp, moveToTrigger2, moveToBridge, moveOverBridge, moveToButton2 };
            evaluationManager = new EvaluationManager();
            foreach (var agent in agents)
            {
                agent.useCBF = useCBF;
                agent.evaluationManager = evaluationManager;
            }
            agentSwitcher = new AgentSwitcher();
            agentSwitcher.AddAgents(agents);

            stepCount = 0;
            compositeEpisodeCount = 0;

            InitCBFs();
            InitTree();
            var actions = _tree.FindNodes<LearningActionAgentSwitcher>();

            var runId = "testRunId";

            if (evaluationActive)
            {
                evaluationManager.Init(this, conditions, agents, actions, runId);
            }
            Debug.Log("MyBTTest Start done");
        }

        private void InitTree()
        {
            // conditions
            var isControllingT1 = new Condition("T1", controller.IsControllingT1);
            var playerUp = new Condition("Up", controller.env.PlayerUp);
            var b1Pressed = new Condition("B1", controller.env.Button1Pressed);
            var isControllingT2 = new Condition("T2", controller.IsControllingT2);
            var B2Pressed = new Condition("B2", controller.env.button2Pressed);
            var onBridge = new Condition("OnBridge", controller.env.PlayerOnBridge);
            var playerPastX3 = new Condition("PastBridge", controller.env.PlayerRightOfX3);
            conditions = new List<Condition> { isControllingT1, playerUp, b1Pressed, isControllingT2, B2Pressed, onBridge, playerPastX3 };

            _tree = new BT(
                new Sequence("Root", new Node[] {
                    new Selector("Selector", new Node[] {
                        new PredicateCondition("B2", B2Pressed),
                        new Sequence("Sequence", new Node[]{

                            new Selector("Selector", new Node[] {
                                new PredicateCondition("B1", b1Pressed),
                                new Sequence("Sequence", new Node[]{

                                    new Selector("Selector", new Node[]{
                                        new PredicateCondition("T1", isControllingT1),
                                        new LearningActionAgentSwitcher("MoveToT1", moveToTrigger1, agentSwitcher, isControllingT1),
                                    } ),

                                    new Selector("Selector", new Node[]{
                                        new PredicateCondition("Up", playerUp),
                                        new LearningActionAgentSwitcher("MoveUp", moveUp, agentSwitcher, playerUp, new List<Condition> {isControllingT1}),
                                    } ),

                                    new LearningActionAgentSwitcher("MoveToB1", moveToButton1, agentSwitcher, b1Pressed, new List<Condition> {playerUp})
                                }),
                            }),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("T2", isControllingT2),
                                new LearningActionAgentSwitcher("MoveToT2", moveToTrigger2, agentSwitcher, isControllingT2, new List<Condition> {b1Pressed}),
                            }),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("Up2", playerUp),
                                new LearningActionAgentSwitcher("MoveUp2", moveUp, agentSwitcher, playerUp, new List<Condition> {b1Pressed}),
                            }),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("PastBridge", playerPastX3),
                                new Sequence("Sequence", new Node[]{

                                    new Selector("Selector", new Node[]{
                                        new PredicateCondition("OnBridge", onBridge),
                                        new LearningActionAgentSwitcher("MoveToBridge", moveToBridge, agentSwitcher, onBridge, new List<Condition> {b1Pressed, playerUp})
                                    }),

                                    new LearningActionAgentSwitcher("MoveOverBridge", moveOverBridge, agentSwitcher, playerPastX3, new List<Condition> {b1Pressed, playerUp, onBridge})
                                })
                            }),

                            new LearningActionAgentSwitcher("MoveToB2", moveToButton2, agentSwitcher, B2Pressed, new List<Condition> {b1Pressed, playerUp, playerPastX3})
                        }),
                    }),

                    new Do("Success", () =>
                    {
                        Debug.Log("Success Reset!");
                        evaluationManager.AddEvent(new GlobalSuccessEvent());
                        NextEpisode();
                        return TaskStatus.Success;
                    })
                })
            );
        }

        // TODO: move CBFs to learning action nodes instead of agents to be able to use the same agent with different CBFs
        private void InitCBFs()
        {
            int steps = moveToTrigger1.ActionsPerDecision;
            float deltaTime = Time.fixedDeltaTime * (steps);
            float eta = 1f;
            System.Func<float, float> alpha = ((float x) => x);
            float margin = Utility.eps + 0.2f;
            bool debugCBF = false;
            float maxAccFactor = 1 / 1.5f;
            float maxAcc = controller.MaxAcc * maxAccFactor;

            var leftOfX1CBF = new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X1, controller.env.ElevatedGroundY, 0), new Vector3(-1, 0, 0), maxAcc, margin);
            var rightOfX1CBF = new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X1, controller.env.ElevatedGroundY, 0), new Vector3(1, 0, 0), maxAcc, margin);
            var rightOfX3CBF = new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X3, controller.env.ElevatedGroundY, 0), new Vector3(1, 0, 0), maxAcc, margin);
            var buttonPressedCBF = new StaticPointCBF3D2ndOrderApproximation(maxAcc, controller.env.PlayerScale + margin);
            var topEdgeBridgeCBF = new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, controller.env.BridgeWidth / 2), new Vector3(0, 0, -1), maxAcc, margin);
            var bottomEdgeBridgeCBF = new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, -controller.env.BridgeWidth / 2), new Vector3(0, 0, 1), maxAcc, margin);
            var bridgeOpenLeftRightCBF = new MinCBF(new List<ICBF> { topEdgeBridgeCBF, bottomEdgeBridgeCBF });
            var upCBF = new MaxCBF(new List<ICBF> { leftOfX1CBF, rightOfX3CBF });
            var upBridgeCBF = new MaxCBF(new List<ICBF> { upCBF, bridgeOpenLeftRightCBF });
            var bridgeOpenRightCBF = new MinCBF(new List<ICBF> { rightOfX1CBF, topEdgeBridgeCBF, bottomEdgeBridgeCBF });

            var moveToButton1_posVelDynamics = new PlayerPosVelDynamics(moveToButton1);
            var moveToButton1_leftOfX1CBFApplicator = new DiscreteCBFApplicator(leftOfX1CBF, moveToButton1_posVelDynamics, deltaTime, debug: debugCBF);
            moveToButton1.CBFApplicators = new List<CBFApplicator> { moveToButton1_leftOfX1CBFApplicator };

            var moveToTrigger2_playerTrigger1PosVelDynamics = new PlayerTrigger1PosVelDynamics(moveToTrigger2);
            var moveToTrigger2_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToTrigger2_playerTrigger1PosVelDynamics, deltaTime, debug: debugCBF);
            moveToTrigger2.CBFApplicators = new List<CBFApplicator> { moveToTrigger2_buttonPressedCBFApplicator };

            var moveToBridge_posVelDynamics = new PlayerPosVelDynamics(moveToBridge);
            var moveToBridge_playerTrigger1PosVelDynamics = new PlayerTrigger1PosVelDynamics(moveToBridge);
            var moveToBridge_upBridgeCBFApplicator = new DiscreteCBFApplicator(upBridgeCBF, moveToBridge_posVelDynamics, deltaTime, debug: debugCBF);
            var moveToBridge_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToBridge_playerTrigger1PosVelDynamics, deltaTime, debug: debugCBF);
            moveToBridge.CBFApplicators = new List<CBFApplicator> { moveToBridge_buttonPressedCBFApplicator, moveToBridge_upBridgeCBFApplicator };

            var moveOverBridge_posVelDynamics = new PlayerPosVelDynamics(moveOverBridge);
            var moveOverBridge_bridgeOpenRightCBFApplicator = new DiscreteCBFApplicator(bridgeOpenRightCBF, moveOverBridge_posVelDynamics, deltaTime, debug: debugCBF);
            moveOverBridge.CBFApplicators = new List<CBFApplicator> { moveOverBridge_bridgeOpenRightCBFApplicator };

            var moveToButton2_posVelDynamics = new PlayerPosVelDynamics(moveToButton2);
            var moveToButton2_pastBridgeCBFApplicator = new DiscreteCBFApplicator(rightOfX3CBF, moveToButton2_posVelDynamics, deltaTime, debug: debugCBF);
            moveToButton2.CBFApplicators = new List<CBFApplicator> { moveToButton2_pastBridgeCBFApplicator };
        }

        void FixedUpdate()
        {
            // Update our tree every frame
            // Debug.Log("Tick");
            _tree.Tick();
            if (++stepCount % MaxSteps == 0)
            {
                Debug.Log("Global Reset!");
                evaluationManager.AddEvent(new ActionGlobalResetEvent { localStep = this.Agent.ActionCount });
                evaluationManager.AddEvent(new GlobalResetEvent { });
                NextEpisode();
            }
        }

        private void NextEpisode()
        {
            _tree.Reset();
            agentSwitcher.Reset();
            controller.env.Reset();
            stepCount = 0;
            compositeEpisodeCount++;
        }
    }
}
