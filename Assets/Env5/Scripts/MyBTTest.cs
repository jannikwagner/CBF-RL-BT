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
        public int maxSteps;
        public int maxStepsPerLocalEpisode;
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
            var agents = new EnvBaseAgent[] { moveToTrigger1, moveToButton1, moveUp, moveToTrigger2, moveToBridge, moveOverBridge, moveToButton2 };
            evaluationManager = new EvaluationManager();
            foreach (var agent in agents)
            {
                agent.useCBF = useCBF;
                agent.evaluationManager = evaluationManager;
                agent.maxActions = maxStepsPerLocalEpisode;
            }
            agentSwitcher = new AgentSwitcher();
            agentSwitcher.AddAgents(agents);

            stepCount = 0;
            compositeEpisodeCount = 0;

            InitTree();
            var actions = _tree.FindNodes<SwitchedLearningAction>();

            var runId = "testRunId";

            if (evaluationActive)
            {
                evaluationManager.Init(this, conditions, agents, actions, runId);
            }
        }

        private void InitTree()
        {
            // conditions
            var isControllingT1 = new Condition("T1", controller.IsControllingT1);
            var playerUp = new Condition("Up", controller.env.PlayerUp);
            var b1Pressed = new Condition("B1", controller.env.Button1Pressed);
            var isControllingT2 = new Condition("T2", controller.IsControllingT2);
            var b2Pressed = new Condition("B2", controller.env.Button2Pressed);
            var onBridge = new Condition("OnBridge", controller.env.PlayerOnBridge);
            var playerPastX3 = new Condition("PastBridge", controller.env.PlayerRightOfX3);
            conditions = new List<Condition> { isControllingT1, playerUp, b1Pressed, isControllingT2, b2Pressed, onBridge, playerPastX3 };


            int steps = moveToTrigger1.ActionsPerDecision;
            float deltaTime = Time.fixedDeltaTime * steps;
            // float eta = 1f;
            System.Func<float, float> alpha = (float x) => x;
            float margin = Utility.eps + 0.0f;
            bool debugCBF = false;
            float maxAccFactor = 1f / 2f;
            float maxAcc = controller.MaxAcc * maxAccFactor;

            // CBFs
            var leftOfX1CBF = new CBFInitWrapper(() => new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X1, controller.env.ElevatedGroundY, 0), new Vector3(-1, 0, 0), maxAcc, margin));
            var rightOfX1CBF = new CBFInitWrapper(() => new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X1, controller.env.ElevatedGroundY, 0), new Vector3(1, 0, 0), maxAcc, margin));
            var rightOfX3CBF = new CBFInitWrapper(() => new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X3, controller.env.ElevatedGroundY, 0), new Vector3(1, 0, 0), maxAcc, margin));
            var button1PressedCBF = new CBFInitWrapper(() => new StaticPointCBF3D2ndOrderApproximation(maxAcc, controller.env.PlayerScale + margin));
            var northEdgeBridgeCBF = new CBFInitWrapper(() => new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, controller.env.BridgeZ + controller.env.BridgeWidth / 2), new Vector3(0, 0, -1), maxAcc, margin));
            var southEdgeBridgeCBF = new CBFInitWrapper(() => new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, controller.env.BridgeZ - controller.env.BridgeWidth / 2), new Vector3(0, 0, 1), maxAcc, margin));
            var bridgeOpenLeftRightCBF = new MinCBF(new List<ICBF> { northEdgeBridgeCBF, southEdgeBridgeCBF });
            var upCBF = new MaxCBF(new List<ICBF> { leftOfX1CBF, rightOfX3CBF });
            var upBridgeCBF = new MaxCBF(new List<ICBF> { upCBF, bridgeOpenLeftRightCBF });
            var bridgeOpenRightCBF = new MinCBF(new List<ICBF> { rightOfX1CBF, northEdgeBridgeCBF, southEdgeBridgeCBF });

            // Dynamics Providers
            var posVelDynamics = new PlayerPosVelDynamics(this);
            var playerTrigger1PosVelDynamics = new PlayerTrigger1PosVelDynamics(this);

            // CBF Applicators
            var leftOfX1CBFApplicator = new DiscreteCBFApplicator(leftOfX1CBF, posVelDynamics, deltaTime, debug: debugCBF);
            var button1PressedCBFApplicator = new DiscreteCBFApplicator(button1PressedCBF, playerTrigger1PosVelDynamics, deltaTime, debug: debugCBF);
            var upBridgeCBFApplicator = new DiscreteCBFApplicator(upBridgeCBF, posVelDynamics, deltaTime, debug: debugCBF);
            var bridgeOpenRightCBFApplicator = new DiscreteCBFApplicator(bridgeOpenRightCBF, posVelDynamics, deltaTime, debug: debugCBF);
            var pastBridgeCBFApplicator = new DiscreteCBFApplicator(rightOfX3CBF, posVelDynamics, deltaTime, debug: debugCBF);

            // CBF registration
            // moveToButton1.CBFApplicators = new List<CBFApplicator> { leftOfX1CBFApplicator };
            // moveToTrigger2.CBFApplicators = new List<CBFApplicator> { button1PressedCBFApplicator };
            // moveToBridge.CBFApplicators = new List<CBFApplicator> { button1PressedCBFApplicator, upBridgeCBFApplicator };
            // moveOverBridge.CBFApplicators = new List<CBFApplicator> { bridgeOpenRightCBFApplicator };
            // moveToButton2.CBFApplicators = new List<CBFApplicator> { pastBridgeCBFApplicator };

            // moveUp.CBFApplicators = new List<CBFApplicator> { button1PressedCBFApplicator };

            // multi goal
            _tree = new BT(
                new Sequence("Root", new Node[] {
                    new Selector("Selector", new Node[] {
                        new PredicateCondition("B1", b1Pressed),
                        new Sequence("Sequence", new Node[]{

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("T1", isControllingT1),
                                new SwitchedLearningAction("MoveToT1", moveToTrigger1, agentSwitcher, isControllingT1, null, new List<Condition> {b1Pressed}),
                            } ),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("Up", playerUp),
                                new SwitchedLearningAction("MoveUp", moveUp, agentSwitcher, playerUp, new List<Condition> {isControllingT1}, new List<Condition> {b1Pressed}),
                            } ),

                            new SwitchedLearningAction("MoveToB1", moveToButton1, agentSwitcher, b1Pressed, new List<Condition> {playerUp}, null, new List<CBFApplicator>{leftOfX1CBFApplicator})  // isControllingT1
                        }),
                    }),

                    new Selector("Selector", new Node[] {
                        new PredicateCondition("B2", b2Pressed),
                        new Sequence("Sequence", new Node[]{

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("T2", isControllingT2),
                                new SwitchedLearningAction("MoveToT2", moveToTrigger2, agentSwitcher, isControllingT2, new List<Condition> {b1Pressed}, new List<Condition> {b2Pressed}, new List<CBFApplicator>{button1PressedCBFApplicator}),
                            }),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("PastBridge", playerPastX3),
                                new Sequence("Sequence", new Node[]{

                                    new Selector("Selector", new Node[]{
                                        new PredicateCondition("OnBridge", onBridge),

                                        new Sequence("Sequence",new Node[]{
                                            new Selector("Selector", new Node[]{
                                                new PredicateCondition("Up2", playerUp),
                                                new SwitchedLearningAction("MoveUp2", moveUp, agentSwitcher, playerUp, new List<Condition> {b1Pressed}, new List<Condition> {b2Pressed, playerPastX3, onBridge}, new List<CBFApplicator> { button1PressedCBFApplicator }),
                                            }),

                                            new SwitchedLearningAction("MoveToBridge", moveToBridge, agentSwitcher, onBridge, new List<Condition> {b1Pressed, isControllingT2, playerUp}, new List<Condition> {b2Pressed, playerPastX3}, new List<CBFApplicator> { button1PressedCBFApplicator, upBridgeCBFApplicator })
                                        }),
                                    }),

                                    new SwitchedLearningAction("MoveOverBridge", moveOverBridge, agentSwitcher, playerPastX3, new List<Condition> {b1Pressed, isControllingT2, onBridge}, new List<Condition> {b2Pressed}, new List<CBFApplicator> { bridgeOpenRightCBFApplicator })
                                })
                            }),

                            new SwitchedLearningAction("MoveToB2", moveToButton2, agentSwitcher, b2Pressed, new List<Condition> {b1Pressed, playerPastX3}, null, new List<CBFApplicator> { pastBridgeCBFApplicator })  // isControllingT2
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
            Debug.Log(_tree.printTree());
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
