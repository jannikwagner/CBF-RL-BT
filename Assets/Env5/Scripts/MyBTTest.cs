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
        public MoveOverBridge moveOverBridge;
        public MovePlayerUp moveUp;
        public MoveToBridge moveToBridge;
        public MoveToTarget moveToT1;
        public PushTargetToButton moveToB1;
        public MoveToGoalTrigger moveToT2;
        public PushTriggerToGoalNew moveToB2;
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
            var agents = new EnvBaseAgent[] { moveToT1, moveToB1, moveUp, moveToT2, moveToBridge, moveOverBridge, moveToB2 };
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
            var b1Pressed = new Condition("B1", controller.env.ButtonPressed);
            var isControllingT2 = new Condition("T2", controller.IsControllingT2);
            var B2Pressed = new Condition("B2", controller.env.GoalPressed);
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
                                        new LearningActionAgentSwitcher("MoveToT1", moveToT1, agentSwitcher, isControllingT1),
                                    } ),

                                    new Selector("Selector", new Node[]{
                                        new PredicateCondition("TargetUp", playerUp),
                                        new LearningActionAgentSwitcher("MoveUp", moveUp, agentSwitcher, playerUp, new List<Condition> {isControllingT1}),
                                    } ),

                                    new LearningActionAgentSwitcher("MoveToB1", moveToB1, agentSwitcher, b1Pressed, new List<Condition> {playerUp})
                                }),
                            }),

                            new Selector("Selector", new Node[]{
                                new PredicateCondition("T2", isControllingT2),
                                new LearningActionAgentSwitcher("MoveToT2", moveToT2, agentSwitcher, isControllingT2, new List<Condition> {b1Pressed}),
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

                            new LearningActionAgentSwitcher("MoveToB2", moveToB2, agentSwitcher, B2Pressed, new List<Condition> {b1Pressed, playerUp, playerPastX3})
                        }),
                    }),

                    new Do("Success", () =>
                    {
                        Debug.Log("Success Reset!");
                        // Debug.Log(controller.env.ButtonPressed());
                        // Debug.Log(controller.env.GoalPressed());
                        evaluationManager.AddEvent(new GlobalSuccessEvent());
                        NextEpisode();
                        return TaskStatus.Success;
                    })
                })
            );
            /*
                // _tree = new BT(
                //     new Sequence("Root", new Node[] {
                //         new Selector("PushTriggerToGoalSelector", new Node[] {
                //             new PredicateCondition("TriggerAtGoal", controller.env.win),
                //             new Sequence("PushTriggerToGoalSequence", new Node[]{

                //                 new Selector("PushTargetToButtonSelector", new Node[] {
                //                     new PredicateCondition("TargetAtButton", controller.env.ButtonPressed),
                //                     new Sequence("PushTargetToButtonSequence", new Node[]{

                //                         new Selector("MoveSelector", new Node[]{
                //                             new PredicateCondition("CloseToTarget", controller.IsCloseToTarget),
                //                             new LearningActionWPC("MoveToTarget", moveToTarget, controller.IsCloseToTarget),
                //                         } ),

                //                         new Selector("PushTargetUpSelector", new Node[]{
                //                             new PredicateCondition("TargetUp", controller.env.TargetUp),
                //                             new LearningActionWPCACC("PushTargetUp", pushTargetUp, controller.env.TargetUp, new System.Func<bool>[] {controller.IsCloseToTarget}),
                //                         } ),

                //                         new LearningActionWPCACC("PushTargetToButton", pushTargetToButton, controller.env.ButtonPressed, new System.Func<bool>[] {controller.IsCloseToTarget, controller.env.TargetUp})
                //                     }),
                //                 }),

                //                 new Selector("MoveToGoalTriggerSelector", new Node[]{
                //                     new PredicateCondition("CloseToTrigger", controller.IsCloseToGoalTrigger),
                //                     new LearningActionWPCACC("MoveToTrigger", moveToGoalTrigger, controller.IsCloseToGoalTrigger, new System.Func<bool>[] {controller.env.ButtonPressed}),
                //                 }),

                //                 new LearningActionWPCACC("PushTriggerToGoal", pushTriggerToGoal, controller.env.win, new System.Func<bool>[] {controller.IsCloseToGoalTrigger, controller.env.ButtonPressed})
                //             }),
                //         }),

                //         new Do("SuccessMessage", () =>
                //         {
                //             Debug.Log("Success!");
                //             return TaskStatus.Success;
                //         })
                //     })
                // );

                // _tree = new BT(
                //     new Sequence("Root", new Node[] {
                //         new Selector("PushTargetToButtonSelector", new Node[] {
                //             new PredicateCondition("TargetAtGoal", controller.env.ButtonPressed),
                //             new Sequence("PushTargetToButtonSequence", new Node[]{
                //                 new Selector("MoveSelector", new Node[]{
                //                     new PredicateCondition("CloseToTarget", controller.IsCloseToTarget),
                //                     new LearningActionWPC("MoveToTarget", moveToTarget, controller.IsCloseToTarget),
                //                 } ),
                //                 new Selector("PushTargetUpSelector", new Node[]{
                //                     new PredicateCondition("TargetUp", controller.env.TargetUp),
                //                     new LearningActionWPCACC("PushTargetUp", pushTargetUp, controller.env.TargetUp, new System.Func<bool>[] {controller.IsCloseToTarget}),
                //                 } ),
                //                 new LearningActionWPCACC("PushTargetToButton", pushTargetToButton, controller.env.ButtonPressed, new System.Func<bool>[] {controller.IsCloseToTarget, controller.env.TargetUp})
                //             }),
                //         }),
                //         new Do("SuccessMessage", () =>
                //         {
                //             Debug.Log("Success!");
                //             return TaskStatus.Success;
                //         })
                //     })
                // );

                // _tree = new BT(
                //     new Sequence("Root", new Node[] {
                //         new Selector("PushSelector", new Node[] {
                //             new PredicateCondition("TargetAtGoal", controller.env.ButtonPressed),
                //             new Sequence("PushSequence", new Node[]{
                //                 new Selector("MoveSelector", new Node[]{
                //                     new PredicateCondition("CloseToTarget", controller.IsCloseToTarget),
                //                     new LearningActionWPC("MoveToTarget", moveToTarget, controller.IsCloseToTarget),
                //                 } ),
                //                 new LearningActionWPCACC("PushTargetToButton", pushTargetToButton, controller.env.ButtonPressed, new System.Func<bool>[] {controller.IsCloseToTarget})
                //             }),
                //         }),
                //         new Do("SuccessMessage", () =>
                //         {
                //             Debug.Log("Success!");
                //             return TaskStatus.Success;
                //         })
                //     })
                // );

                // int count1 = 0;
                // int count2 = 0;
                // _tree = new BT(
                //     new Sequence("Root", new Node[]{
                //         new Selector("Selector", new Node[]{
                //             new PredicateCondition("Count1", () =>
                //             {
                //                 count1++;
                //                 Debug.Log("Count1: " + count1);
                //                 return (count1 > 5);
                //             }),
                //             new Do("Count2", () =>
                //             {
                //                 count2++;
                //                 Debug.Log("Count2: " + count2);
                //                 if (count2 > 10)
                //                 {
                //                     return TaskStatus.Success;
                //                 }
                //                 return TaskStatus.Running;
                //             }),
                //         }),
                //         new Do("SuccessMessage", () =>
                //         {
                //             Debug.Log("Success!");
                //             return TaskStatus.Success;
                //         })
                //     })
                // );

                // int count3 = 0;
                // _tree = new BT(
                //     new Sequence("Root", new Node[]{
                //         new PredicateCondition("Switcher", () => {var condition = ++count3 % 3 != 0; Debug.Log(condition); return condition;}),
                //         new PrintAction("CustomAction"),
                //         new Do("SuccessMessage", () =>
                //         {
                //             Debug.Log("Success!");
                //             return TaskStatus.Success;
                //         })
                //     })
                // );
            */
        }

        // TODO: move CBFs to learning action nodes instead of agents to be able to use the same agent with different CBFs
        private void InitCBFs()
        {
            int steps = moveToT1.ActionsPerDecision;
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

            var pushTargetToButtonPosVelDynamics = new PlayerPosVelDynamics(moveToB1);
            var pushTargetToButton_leftOfX1CBFApplicator = new DiscreteCBFApplicator(leftOfX1CBF, pushTargetToButtonPosVelDynamics, deltaTime, debug: debugCBF);
            moveToB1.CBFApplicators = new List<CBFApplicator> { pushTargetToButton_leftOfX1CBFApplicator };

            var moveToGoalTriggerPlayerTargetPosVelDynamics = new PlayerTargetPosVelDynamics(moveToT2);
            var moveToGoalTrigger_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToGoalTriggerPlayerTargetPosVelDynamics, deltaTime, debug: debugCBF);
            moveToT2.CBFApplicators = new List<CBFApplicator> { moveToGoalTrigger_buttonPressedCBFApplicator };

            var moveToBridgePosVelDynamics = new PlayerPosVelDynamics(moveToBridge);
            var moveToBridgePlayerTargetPosVelDynamics = new PlayerTargetPosVelDynamics(moveToBridge);
            var moveToBridge_upBridgeCBFApplicator = new DiscreteCBFApplicator(upBridgeCBF, moveToBridgePosVelDynamics, deltaTime, debug: debugCBF);
            var moveToBridge_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToBridgePlayerTargetPosVelDynamics, deltaTime, debug: debugCBF);
            moveToBridge.CBFApplicators = new List<CBFApplicator> { moveToBridge_buttonPressedCBFApplicator, moveToBridge_upBridgeCBFApplicator };

            var moveOverBridgePosVelDynamics = new PlayerPosVelDynamics(moveOverBridge);
            var moveOverBridge_bridgeOpenRightCBFApplicator = new DiscreteCBFApplicator(bridgeOpenRightCBF, moveOverBridgePosVelDynamics, deltaTime, debug: debugCBF);
            moveOverBridge.CBFApplicators = new List<CBFApplicator> { moveOverBridge_bridgeOpenRightCBFApplicator };

            var pushTriggerToGoalNewPosVelDynamics = new PlayerPosVelDynamics(moveToB2);
            var pushTriggerToGoalNew_pastBridgeCBFApplicator = new DiscreteCBFApplicator(rightOfX3CBF, pushTriggerToGoalNewPosVelDynamics, deltaTime, debug: debugCBF);
            moveToB2.CBFApplicators = new List<CBFApplicator> { pushTriggerToGoalNew_pastBridgeCBFApplicator };

            // var upBridgeCBFPosVelDynamics = new PlayerPosVelDynamics(pushTriggerToGoal);
            // var upBridgeCBFApplicator = new DiscreteCBFApplicator(upBridgeCBF, upBridgeCBFPosVelDynamics, debug: debugCBF);
            // var buttonPressedCBFPosVelDynamics2 = new PlayerTargetPosVelDynamics(pushTriggerToGoal);
            // var buttonPressedCBFApplicator2 = new DiscreteCBFApplicator(buttonPressedCBF, buttonPressedCBFPosVelDynamics2, debug: debugCBF);
            // pushTriggerToGoal.CBFApplicators = new List<CBFApplicator> { buttonPressedCBFApplicator2, upBridgeCBFApplicator };
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
