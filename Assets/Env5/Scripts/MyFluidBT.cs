using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using Unity.MLAgents;
using CleverCrow.Fluid.BTs.TaskParents.Composites;
namespace Env5
{
    public class MyFluidBT : MonoBehaviour
    {
        [SerializeField]
        private BehaviorTree _tree;
        public IAgentSwitcher agentSwitcher;
        public MoveToTarget moveToTarget;
        public PushTargetToButton pushTargetToButton;
        public PlayerController controller;

        private void Awake()
        {
            _tree = new BehaviorTreeBuilder(gameObject)
                .Sequence("Root")
                    .Selector("PushSelector")
                        .Condition("TargetAtGoal", controller.env.ButtonPressed)
                        .Sequence("PushSequence")
                            .Selector("MoveSelector")
                                .Condition("CloseToTarget", controller.IsCloseToTarget)
                                .LearningActionWPC("MoveToTarget", moveToTarget, controller.IsCloseToTarget)
                            .End()
                            .LearningActionWPC("PushTargetToButton", pushTargetToButton, controller.env.ButtonPressed)
                        .End()
                    .End()
                    .Do("SuccessMessage", () =>
                    {
                        Debug.Log("Success!");
                        return TaskStatus.Success;
                    })
                .End()
                .Build();

            // int count1 = 0;
            // int count2 = 0;

            // _tree = new BehaviorTreeBuilder(gameObject)
            //     .Sequence("Root")
            //         .Selector("Selector")
            //             .Condition("Count1", () =>
            //             {
            //                 count1++;
            //                 Debug.Log("Count1: " + count1);
            //                 return (count1 > 5);
            //             })
            //             .Do("Count2", () =>
            //             {
            //                 count2++;
            //                 Debug.Log("Count2: " + count2);
            //                 if (count2 > 10)
            //                 {
            //                     return TaskStatus.Success;
            //                 }
            //                 return TaskStatus.Continue;
            //             })
            //         .End()
            //         .Do("SuccessMessage", () =>
            //         {
            //             Debug.Log("Success!");
            //             return TaskStatus.Success;
            //         })
            //     .End()
            //     .Build();

            // _tree = new BehaviorTreeBuilder(gameObject)
            //     .Sequence("Root")
            //         .CustomAction("CustomAction")
            //         .Do("SuccessMessage", () =>
            //         {
            //             Debug.Log("Success!");
            //             return TaskStatus.Success;
            //         })
            //     .End()
            //     .Build();
        }

        private void Update()
        {
            // Update our tree every frame
            _tree.Tick();
            _tree.Reset();
        }
    }

    public class CustomAction : ActionBase
    {

        protected override void OnInit()
        {
            base.OnInit();
            Debug.Log("CustomAction.OnInit");
        }

        protected override TaskStatus OnUpdate()
        {
            Debug.Log("CustomAction.OnUpdate");
            return TaskStatus.Continue;
        }

        protected override TaskStatus GetUpdate()
        {
            Debug.Log("CustomAction.GetUpdate");
            return base.GetUpdate();
        }

        protected override void OnStart()
        {
            Debug.Log("CustomAction.OnStart");
            base.OnStart();
        }

        protected override void OnExit()
        {
            Debug.Log("CustomAction.OnExit");
            base.OnExit();
        }
    }

    public static class BehaviorTreeBuilderExtensions
    {
        public static BehaviorTreeBuilder CustomAction(this BehaviorTreeBuilder builder, string name)
        {
            return builder.AddNode(new CustomAction
            {
                Name = name,
            });
        }
        public static BehaviorTreeBuilder LearningAction(this BehaviorTreeBuilder builder, string name, EnvBaseAgent agent)
        {
            return builder.AddNode(new LearningAction
            {
                Name = name,
                Agent = agent,
            });
        }
        public static BehaviorTreeBuilder LearningActionWPC(this BehaviorTreeBuilder builder, string name, EnvBaseAgent agent, System.Func<bool> postCondition)
        {
            return builder.AddNode(new LearningActionWithPostCondition
            {
                Name = name,
                Agent = agent,
                PostCondition = postCondition,
            });
        }
        public static BehaviorTreeBuilder CustomSequence(this BehaviorTreeBuilder builder, string name = "My Sequence")
        {
            return builder.ParentTask<CustomSequence>(name);
        }
        public static BehaviorTreeBuilder CustomSelector(this BehaviorTreeBuilder builder, string name = "My Selector")
        {
            return builder.ParentTask<CustomSelector>(name);
        }
    }

    public class LearningAction : ActionBase
    {
        private EnvBaseAgent agent;
        private int stepsPerDecision = 10;
        private int stepCount = 0;
        private int maxSteps = 1000;

        public EnvBaseAgent Agent { get => agent; set => agent = value; }
        public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        protected override TaskStatus OnUpdate()
        {
            // Debug.Log(Name + ": OnUpdate");
            stepCount++;
            if (stepCount >= StepsPerDecision)
            {
                stepCount = 0;
                Agent.RequestDecision();
            }
            else
            {
                Agent.RequestAction();
            }

            if (Agent.StepCount >= MaxSteps)
            {
                Agent.EndEpisode();
                Agent.controller.env.Initialize();
            }

            return TaskStatus.Continue;
        }
    }

    public class LearningActionWithPostCondition : LearningAction
    {
        private System.Func<bool> postCondition;

        public System.Func<bool> PostCondition { get => postCondition; set => postCondition = value; }

        protected override TaskStatus OnUpdate()
        {
            var status = base.OnUpdate();
            // if (PostCondition())
            Debug.Log(Name + ": PostCondition: " + PostCondition());
            if (status == TaskStatus.Continue && PostCondition())
            {
                Debug.Log(Name + ": PostCondition met");
                Agent.AddReward(1f);
                Agent.EndEpisode();
                return TaskStatus.Success;
            }
            return status;
        }
    }

    public class CustomSequence : CompositeBase
    {
        protected override TaskStatus OnUpdate()
        {
            for (var i = ChildIndex; i < Children.Count; i++)
            {
                var child = Children[ChildIndex];

                var status = child.Update();
                if (status != TaskStatus.Success)
                {
                    return status;
                }

                ChildIndex++;
            }

            return TaskStatus.Success;
        }
    }

    public class CustomSelector : CompositeBase
    {
        protected override TaskStatus OnUpdate()
        {
            for (var i = ChildIndex; i < Children.Count; i++)
            {
                var child = Children[ChildIndex];

                var status = child.Update();
                if (status != TaskStatus.Failure)
                {
                    return status;
                }

                ChildIndex++;
            }

            return TaskStatus.Failure;
        }
    }
}
