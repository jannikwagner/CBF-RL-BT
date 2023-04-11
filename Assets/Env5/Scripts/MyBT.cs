using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using Unity.MLAgents;

public class MyBT : MonoBehaviour
{
    [SerializeField]
    private BehaviorTree _tree;
    public AgentSwitcher agentSwitcher;
    public EnvController envController;
    public MoveToTarget moveToTarget;
    public PushTargetToButton pushTargetToButton;
    public PlayerController controller;

    private void Awake()
    {
        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence("Root")
                .Selector("PushSelector")
                    .Condition("TargetAtGoal", () =>
                    {
                        return envController.buttonsPressed();
                    })
                    .Sequence("PushSequence")
                        .Selector("MoveSelector")
                            .Condition("CloseToTarget", () =>
                            {
                                return controller.isCloseToTarget();
                            })
                            .LearningAction("MoveToTarget", moveToTarget)
                        .End()
                    .End()
                    .LearningAction("PushTargetToButton", pushTargetToButton)
                .End()
                .Do("SuccessMessage", () =>
                {
                    Debug.Log("Success!");
                    return TaskStatus.Success;
                })
            .End()
            .Build();

    }

    private void Update()
    {
        // Update our tree every frame
        _tree.Tick();
    }


}

public class CustomAction : ActionBase
{

    protected override void OnInit()
    {
        Debug.Log("CustomAction.OnInit");
    }

    protected override TaskStatus OnUpdate()
    {
        Debug.Log("CustomAction.OnUpdate");
        return TaskStatus.Success;
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
    public static BehaviorTreeBuilder LearningAction(this BehaviorTreeBuilder builder, string name, Agent agent)
    {
        return builder.AddNode(new LearningAction
        {
            Name = name,
            Agent = agent,
        });
    }
}

public class LearningAction : ActionBase
{
    private Agent agent;
    private int stepsPerDecision = 10;
    private int stepCount = 0;
    private int MaxSteps = 1000;

    public Agent Agent { get => agent; set => agent = value; }
    public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
    public int MaxSteps1 { get => MaxSteps; set => MaxSteps = value; }

    protected override void OnStart()
    {
        base.OnStart();
        stepCount = 0;
        Debug.Log(Name + " OnStart");
    }

    protected override TaskStatus OnUpdate()
    {
        stepCount++;
        if (stepCount >= StepsPerDecision)
        {
            stepCount = 0;
            Agent.RequestDecision();
        }

        return TaskStatus.Continue;
    }

    protected override void OnExit()
    {
        base.OnExit();
        Debug.Log(Name + " OnExit");
        agent.EndEpisode();
    }
}
