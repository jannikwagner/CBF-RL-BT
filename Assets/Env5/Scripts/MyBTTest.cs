using UnityEngine;
using Unity.MLAgents;
using BTTest;
using System.Collections.Generic;

namespace Env5
{
    public class MyBTTest : MonoBehaviour
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
        public PushTriggerToGoal pushTriggerToGoal;
        public PushTriggerToGoalNew pushTriggerToGoalNew;
        public PlayerController controller;
        private int maxSteps = 10000;
        private int stepCount;

        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        private void Start()
        {
            int steps = moveToTarget.ActionsPerDecision;
            float deltaTime = Time.fixedDeltaTime * steps;
            float eta = 1f;
            System.Func<float, float> alpha = ((float x) => x / deltaTime);
            float margin = Utility.eps + 0.2f;
            bool debugCBF = false;
            float maxAccFactor = 1 / 1.5f;
            float maxAcc = controller.MaxAcc * maxAccFactor;

            var upLeftCBF = new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X1, controller.env.ElevatedGroundY, 0), new Vector3(-1, 0, 0), maxAcc, margin);
            var upCBFPosVelDynamics = new PlayerPosVelDynamics(pushTargetToButton);
            var upCBFApplicator = new ContinuousCBFApplicator(upLeftCBF, upCBFPosVelDynamics, debug: debugCBF, alpha: alpha);
            pushTargetToButton.CBFApplicators = new List<CBFApplicator> { upCBFApplicator };

            var buttonPressedCBF = new StaticPointCBF3D2ndOrderApproximation(maxAcc, controller.env.PlayerScale + margin);
            var buttonPressedCBFPosVelDynamics = new PlayerTargetPosVelDynamics(moveToGoalTrigger);
            var buttonPressedCBFApplicator = new ContinuousCBFApplicator(buttonPressedCBF, buttonPressedCBFPosVelDynamics, debug: debugCBF, alpha: alpha);
            moveToGoalTrigger.CBFApplicators = new List<CBFApplicator> { buttonPressedCBFApplicator };

            var bridgeLeftCBF = new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, controller.env.BridgeWidth / 2), new Vector3(0, 0, -1), maxAcc, margin);
            var bridgeRightCBF = new StaticWallCBF3D2ndOrder(new Vector3(0, controller.env.ElevatedGroundY, -controller.env.BridgeWidth / 2), new Vector3(0, 0, 1), maxAcc, margin);
            var bridgeCBF = new MinCBF(new List<ICBF> { bridgeLeftCBF, bridgeRightCBF });
            var upRightCBF = new StaticWallCBF3D2ndOrder(new Vector3(controller.env.X3, controller.env.ElevatedGroundY, 0), new Vector3(1, 0, 0), maxAcc, margin);
            var upCBF = new MaxCBF(new List<ICBF> { upLeftCBF, upRightCBF });
            var upBridgeCBF = new MaxCBF(new List<ICBF> { upCBF, bridgeCBF });
            var upBridgeCBFPosVelDynamics = new PlayerPosVelDynamics(pushTriggerToGoal);
            var upBridgeCBFApplicator = new ContinuousCBFApplicator(upBridgeCBF, upBridgeCBFPosVelDynamics, debug: debugCBF, alpha: alpha);
            var buttonPressedCBFPosVelDynamics2 = new PlayerTargetPosVelDynamics(pushTriggerToGoal);
            var buttonPressedCBFApplicator2 = new ContinuousCBFApplicator(buttonPressedCBF, buttonPressedCBFPosVelDynamics2, debug: debugCBF, alpha: alpha);
            pushTriggerToGoal.CBFApplicators = new List<CBFApplicator> { buttonPressedCBFApplicator2, upBridgeCBFApplicator };

            // conditions
            var isControllingTarget = new Condition("IsControllingTarget", controller.IsControllingTarget);
            var playerUp = new Condition("PlayerUp", controller.env.PlayerUp);
            var buttonPressed = new Condition("ButtonPressed", controller.env.ButtonPressed);
            var isControllingGoalTrigger = new Condition("IsControllingGoalTrigger", controller.IsControllingGoalTrigger);
            var goalPressed = new Condition("GoalPressed", controller.env.GoalPressed);

            var agents = new EnvBaseAgent[] { moveToTarget, pushTargetToButton, movePlayerUp, moveToGoalTrigger, pushTriggerToGoal };
            agentSwitcher = new AgentSwitcher();
            agentSwitcher.AddAgents(agents);

            stepCount = 0;

            _tree = new BT(
                new Sequence("Root", new Node[] {
                    new Selector("PushTriggerToGoalSelector", new Node[] {
                        new PredicateCondition("TriggerAtGoal", controller.env.GoalPressed),
                        new Sequence("PushTriggerToGoalSequence", new Node[]{

                            new Selector("PushTargetToButtonSelector", new Node[] {
                                new PredicateCondition("TargetAtButton", controller.env.ButtonPressed),
                                new Sequence("PushTargetToButtonSequence", new Node[]{

                                    new Selector("MoveSelector", new Node[]{
                                        new PredicateCondition("CloseToTarget", controller.IsControllingTarget),
                                        new LearningActionAgentSwitcher("MoveToTarget", moveToTarget, agentSwitcher, isControllingTarget),
                                    } ),

                                    new Selector("PushTargetUpSelector", new Node[]{
                                        new PredicateCondition("TargetUp", controller.env.PlayerUp),
                                        new LearningActionAgentSwitcher("PushTargetUp", movePlayerUp, agentSwitcher, playerUp, new List<Condition> {isControllingTarget}),
                                    } ),

                                    new LearningActionAgentSwitcher("PushTargetToButton", pushTargetToButton, agentSwitcher, buttonPressed, new List<Condition> {playerUp})
                                }),
                            }),

                            new Selector("MoveToGoalTriggerSelector", new Node[]{
                                new PredicateCondition("CloseToTrigger", controller.IsControllingGoalTrigger),
                                new LearningActionAgentSwitcher("MoveToTrigger", moveToGoalTrigger, agentSwitcher, isControllingGoalTrigger, new List<Condition> {buttonPressed}),
                            }),

                            new Selector("MovePlayerUpSelector", new Node[]{
                                new PredicateCondition("PlayerUp", controller.env.PlayerUp),
                                new LearningActionAgentSwitcher("MovePlayerUp", movePlayerUp, agentSwitcher, playerUp, new List<Condition> {buttonPressed}),
                            } ),

                            new LearningActionAgentSwitcher("PushTriggerToGoal", pushTriggerToGoal, agentSwitcher, goalPressed, new List<Condition> {buttonPressed, playerUp})
                        }),
                    }),

                    new Do("Reset", () =>
                    {
                        Debug.Log("Success Reset!");
                        // Debug.Log(controller.env.ButtonPressed());
                        // Debug.Log(controller.env.GoalPressed());
                        Reset();
                        return TaskStatus.Success;
                    })
                })
            );

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

        }

        void FixedUpdate()
        {
            // Update our tree every frame
            // Debug.Log("Tick");
            _tree.Tick();
            if (++stepCount % MaxSteps == 0)
            {
                Debug.Log("Global Reset!");
                Reset();
            }
        }

        private void Reset()
        {
            _tree.Reset();
            agentSwitcher.Reset();
            controller.env.Reset();
            stepCount = 0;
        }
    }
}
