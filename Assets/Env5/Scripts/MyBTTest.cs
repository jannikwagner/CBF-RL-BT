using UnityEngine;
using Unity.MLAgents;
using BTTest;

public class MyBTTest : MonoBehaviour
{
    [SerializeField]
    private BT _tree;
    public AgentSwitcher agentSwitcher;
    public MoveToTarget moveToTarget;
    public PushTargetToButton pushTargetToButton;
    public PlayerController controller;

    private void Awake()
    {
        _tree = new BT(
            new Sequence("Root", new Node[] {
                new Selector("PushSelector", new Node[] {
                    new PredicateCondition("TargetAtGoal", controller.env.ButtonPressed),
                    new Sequence("PushSequence", new Node[]{
                        new Selector("MoveSelector", new Node[]{
                            new PredicateCondition("CloseToTarget", controller.IsCloseToTarget),
                            new LearningActionWPC("MoveToTarget", moveToTarget, controller.IsCloseToTarget),
                        } ),
                        new LearningActionWPC("PushTargetToButton", pushTargetToButton, controller.env.ButtonPressed)
                    }),
                }),

                new Do("SuccessMessage", () =>
                {
                    Debug.Log("Success!");
                    return TaskStatus.Success;
                })
            })
        );

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

    private void Update()
    {
        // Update our tree every frame
        // Debug.Log("Tick");
        _tree.Tick();
    }
}

public class PrintAction : Action
{
    public PrintAction(string name) : base(name) { }

    public override void OnInit()
    {
        base.OnInit();
        Debug.Log("PrintAction.OnInit");
    }

    public override TaskStatus OnUpdate()
    {
        Debug.Log("PrintAction.OnUpdate");
        return TaskStatus.Running;
    }

    public override void OnStartRunning()
    {
        Debug.Log("PrintAction.OnStartRunning");
        base.OnStartRunning();
    }

    public override void OnStartExecution()
    {
        Debug.Log("PrintAction.OnStartExecution");
        base.OnStartRunning();
    }

    public override void OnStopExecution()
    {
        Debug.Log("PrintAction.OnStopExecution");
        base.OnStartRunning();
    }

    public override void OnStopRunning()
    {
        Debug.Log("PrintAction.OnStopRunning");
        base.OnStartRunning();
    }
}