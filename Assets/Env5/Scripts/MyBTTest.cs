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
        public MoveToTarget moveToTarget;
        public PushTargetToButton pushTargetToButton;
        public PushTargetUp pushTargetUp;
        public MoveToGoalTrigger moveToGoalTrigger;
        public PushTriggerToGoal pushTriggerToGoal;
        public PlayerController controller;
        private int maxSteps = 10000;
        private int stepCount;

        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        private void Awake()
        {
            var agents = new EnvBaseAgent[] { moveToTarget, pushTargetToButton, pushTargetUp, moveToGoalTrigger, pushTriggerToGoal };

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
                                        new PredicateCondition("CloseToTarget", controller.IsCloseToTarget),
                                        new LearningActionAgentSwitcher("MoveToTarget", moveToTarget, agentSwitcher, controller.IsCloseToTarget),
                                    } ),

                                    new Selector("PushTargetUpSelector", new Node[]{
                                        new PredicateCondition("TargetUp", controller.env.TargetUp),
                                        new LearningActionAgentSwitcher("PushTargetUp", pushTargetUp, agentSwitcher, controller.env.TargetUp),
                                    } ),

                                    new LearningActionAgentSwitcher("PushTargetToButton", pushTargetToButton, agentSwitcher, controller.env.ButtonPressed, new List<System.Func<bool>> {controller.env.TargetUp})
                                }),
                            }),

                            new Selector("MoveToGoalTriggerSelector", new Node[]{
                                new PredicateCondition("CloseToTrigger", controller.IsCloseToGoalTrigger),
                                new LearningActionAgentSwitcher("MoveToTrigger", moveToGoalTrigger, agentSwitcher, controller.IsCloseToGoalTrigger, new List<System.Func<bool>> {controller.env.ButtonPressed}),
                            }),

                            new LearningActionAgentSwitcher("PushTriggerToGoal", pushTriggerToGoal, agentSwitcher, controller.env.GoalPressed, new List<System.Func<bool>> {controller.env.ButtonPressed})
                        }),
                    }),

                    new Do("Reset", () =>
                    {
                        Debug.Log("Success Reset!");
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
