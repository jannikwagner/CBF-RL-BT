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
        public MovePlayerUp movePlayerUp;
        public MoveToBridge moveToBridge;
        public MoveToTarget moveToTarget;
        public PushTargetToButton pushTargetToButton;
        public MoveToGoalTrigger moveToGoalTrigger;
        // public PushTriggerToGoal pushTriggerToGoal;
        public PushTriggerToGoalNew pushTriggerToGoalNew;
        public PlayerController controller;
        private bool useCBF = true;
        private int maxSteps = 10000;
        private int stepCount;
        private int compositeEpisodeCount;
        private IEvaluationManager evaluationManager;
        List<Condition> conditions;

        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        public int Step => stepCount;
        public int Episode => compositeEpisodeCount;
        public BaseAgent Agent => agentSwitcher.Agent;
        public Action Action => _tree.CurrentAction;

        private void Start()
        {
            Debug.Log("MyBTTest Start");
            var agents = new EnvBaseAgent[] { moveToTarget, pushTargetToButton, movePlayerUp, moveToGoalTrigger, moveToBridge, moveOverBridge, pushTriggerToGoalNew };
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

            evaluationManager.Init(this, conditions, agents, actions, runId);
            Debug.Log("MyBTTest Start done");
        }

        private void InitTree()
        {
            // conditions
            var isControllingTarget = new Condition("IsControllingTarget", controller.IsControllingTarget);
            var playerUp = new Condition("PlayerUp", controller.env.PlayerUp);
            var buttonPressed = new Condition("ButtonPressed", controller.env.ButtonPressed);
            var isControllingGoalTrigger = new Condition("IsControllingGoalTrigger", controller.IsControllingGoalTrigger);
            var goalPressed = new Condition("GoalPressed", controller.env.GoalPressed);
            var onBridge = new Condition("OnBridge", controller.env.PlayerAboveBridge);
            var playerPastX3 = new Condition("PlayerPastX3", controller.env.PlayerRightOfX3);
            conditions = new List<Condition> { isControllingTarget, playerUp, buttonPressed, isControllingGoalTrigger, goalPressed, onBridge, playerPastX3 };

            _tree = new BT(
                new Sequence("Root", new Node[] {
                    new Selector("PushTriggerToGoalSelector", new Node[] {
                        new PredicateCondition("TriggerAtGoal", goalPressed),
                        new Sequence("PushTriggerToGoalSequence", new Node[]{

                            new Selector("PushTargetToButtonSelector", new Node[] {
                                new PredicateCondition("TargetAtButton", buttonPressed),
                                new Sequence("PushTargetToButtonSequence", new Node[]{

                                    new Selector("MoveSelector", new Node[]{
                                        new PredicateCondition("IsControllingTarget", isControllingTarget),
                                        new LearningActionAgentSwitcher("MoveToTarget", moveToTarget, agentSwitcher, isControllingTarget),
                                    } ),

                                    new Selector("MovePlayerUpSelector", new Node[]{
                                        new PredicateCondition("TargetUp", playerUp),
                                        new LearningActionAgentSwitcher("MovePlayerUp", movePlayerUp, agentSwitcher, playerUp, new List<Condition> {isControllingTarget}),
                                    } ),

                                    new LearningActionAgentSwitcher("PushTargetToButton", pushTargetToButton, agentSwitcher, buttonPressed, new List<Condition> {playerUp})
                                }),
                            }),

                            new Selector("MoveToGoalTriggerSelector", new Node[]{
                                new PredicateCondition("IsControllingGoalTrigger", isControllingGoalTrigger),
                                new LearningActionAgentSwitcher("MoveToTrigger", moveToGoalTrigger, agentSwitcher, isControllingGoalTrigger, new List<Condition> {buttonPressed}),
                            }),

                            new Selector("MovePlayerUpSelector2", new Node[]{
                                new PredicateCondition("PlayerUp", playerUp),
                                new LearningActionAgentSwitcher("MovePlayerUp2", movePlayerUp, agentSwitcher, playerUp, new List<Condition> {buttonPressed}),
                            }),

                            new Selector("MoveOverBridgeSelector", new Node[]{
                                new PredicateCondition("PastBridge", playerPastX3),
                                new Sequence("MoveOverBridgeSequence", new Node[]{

                                    new Selector("MoveToBridgeSelector", new Node[]{
                                        new PredicateCondition("OnBridge", onBridge),
                                        new LearningActionAgentSwitcher("MoveToBridge", moveToBridge, agentSwitcher, onBridge, new List<Condition> {buttonPressed, playerUp})
                                    }),

                                    new LearningActionAgentSwitcher("MoveOverBridge", moveOverBridge, agentSwitcher, playerPastX3, new List<Condition> {buttonPressed, playerUp, onBridge})
                                })
                            }),

                            new LearningActionAgentSwitcher("PushTriggerToGoalNew", pushTriggerToGoalNew, agentSwitcher, goalPressed, new List<Condition> {buttonPressed, playerUp, playerPastX3})
                            // new LearningActionAgentSwitcher("PushTriggerToGoal", pushTriggerToGoal, agentSwitcher, goalPressed, new List<Condition> {buttonPressed, playerUp})
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
            int steps = moveToTarget.ActionsPerDecision;
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

            var pushTargetToButtonPosVelDynamics = new PlayerPosVelDynamics(pushTargetToButton);
            var pushTargetToButton_leftOfX1CBFApplicator = new DiscreteCBFApplicator(leftOfX1CBF, pushTargetToButtonPosVelDynamics, deltaTime, debug: debugCBF);
            pushTargetToButton.CBFApplicators = new List<CBFApplicator> { pushTargetToButton_leftOfX1CBFApplicator };

            var moveToGoalTriggerPlayerTargetPosVelDynamics = new PlayerTargetPosVelDynamics(moveToGoalTrigger);
            var moveToGoalTrigger_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToGoalTriggerPlayerTargetPosVelDynamics, deltaTime, debug: debugCBF);
            moveToGoalTrigger.CBFApplicators = new List<CBFApplicator> { moveToGoalTrigger_buttonPressedCBFApplicator };

            var moveToBridgePosVelDynamics = new PlayerPosVelDynamics(moveToBridge);
            var moveToBridgePlayerTargetPosVelDynamics = new PlayerTargetPosVelDynamics(moveToBridge);
            var moveToBridge_upBridgeCBFApplicator = new DiscreteCBFApplicator(upBridgeCBF, moveToBridgePosVelDynamics, deltaTime, debug: debugCBF);
            var moveToBridge_buttonPressedCBFApplicator = new DiscreteCBFApplicator(buttonPressedCBF, moveToBridgePlayerTargetPosVelDynamics, deltaTime, debug: debugCBF);
            moveToBridge.CBFApplicators = new List<CBFApplicator> { moveToBridge_buttonPressedCBFApplicator, moveToBridge_upBridgeCBFApplicator };

            var moveOverBridgePosVelDynamics = new PlayerPosVelDynamics(moveOverBridge);
            var moveOverBridge_bridgeOpenRightCBFApplicator = new DiscreteCBFApplicator(bridgeOpenRightCBF, moveOverBridgePosVelDynamics, deltaTime, debug: debugCBF);
            moveOverBridge.CBFApplicators = new List<CBFApplicator> { moveOverBridge_bridgeOpenRightCBFApplicator };

            var pushTriggerToGoalNewPosVelDynamics = new PlayerPosVelDynamics(pushTriggerToGoalNew);
            var pushTriggerToGoalNew_pastBridgeCBFApplicator = new DiscreteCBFApplicator(rightOfX3CBF, pushTriggerToGoalNewPosVelDynamics, deltaTime, debug: debugCBF);
            pushTriggerToGoalNew.CBFApplicators = new List<CBFApplicator> { pushTriggerToGoalNew_pastBridgeCBFApplicator };

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
