using System;
using UnityEngine;
using System.Collections.Generic;
using Env5;

namespace BTTest
{
    public enum TaskStatus { Success, Failure, Running };

    public class BT
    {
        public BT(Node root) { this.Root = root; }
        private Node root;
        private HashSet<Node> currentExecutionSet;
        private HashSet<Node> previousExecutionSet;
        private HashSet<Node> currentRunningSet;
        private HashSet<Node> previousRunningSet;
        private Node Root { get => root; set { this.root = value; SetReferenceRec(value); } }
        public HashSet<Node> CurrentExecutionSet { get => currentExecutionSet; set => currentExecutionSet = value; }
        public HashSet<Node> PreviousExecutionSet { get => previousExecutionSet; set => previousExecutionSet = value; }
        public HashSet<Node> CurrentRunningSet { get => currentRunningSet; set => currentRunningSet = value; }
        public HashSet<Node> PreviousRunningSet { get => previousRunningSet; set => previousRunningSet = value; }

        private void SetReferenceRec(Node value)
        {
            value.Bt = this;
            if (value is CompositeNode)
            {
                foreach (Node child in (value as CompositeNode).Children)
                {
                    SetReferenceRec(child);
                }
            }
        }
        public TaskStatus Tick()
        {
            currentRunningSet = new HashSet<Node>();
            currentExecutionSet = new HashSet<Node>();
            var status = Root.Tick();
            if (previousRunningSet != null)
            {
                foreach (Node node in previousRunningSet)
                {
                    if (!currentRunningSet.Contains(node))
                    {
                        node.OnStopRunning();
                    }
                }
            }
            if (previousExecutionSet != null)
            {
                foreach (Node node in previousExecutionSet)
                {
                    if (!currentExecutionSet.Contains(node))
                    {
                        node.OnStopExecution();
                    }
                }
            }
            previousRunningSet = currentRunningSet;
            previousExecutionSet = currentExecutionSet;
            return status;
        }
    }

    public class Node
    {
        private bool initialized = false;
        private BT bt;
        private String name;
        public String Name { get => name; set => name = value; }
        private Node parent;
        public Node Parent { get => parent; set => parent = value; }
        public BT Bt { get => bt; set => bt = value; }
        public bool Initialized { get => initialized; set => initialized = value; }

        public virtual TaskStatus OnUpdate() { return TaskStatus.Failure; }
        public virtual void OnStopRunning() { }
        public virtual void OnStartRunning() { }
        public virtual void OnStopExecution() { }
        public virtual void OnStartExecution() { }
        public virtual void OnInit() { }
        public virtual TaskStatus Tick()
        {
            // Debug.Log(name);
            if (!Initialized)
            {
                OnInit();
                Initialized = true;
            }
            Bt.CurrentExecutionSet.Add(this);
            if (Bt.PreviousExecutionSet == null || !Bt.PreviousExecutionSet.Contains(this))
            {
                OnStartExecution();
            }
            if (Bt.PreviousRunningSet == null || !Bt.PreviousRunningSet.Contains(this))
            {
                OnStartRunning();
            }

            TaskStatus status = OnUpdate();

            if (status == TaskStatus.Running)
            {
                Bt.CurrentRunningSet.Add(this);
            }
            // if it were in previous, it would be stopped in BT
            else if (Bt.PreviousRunningSet == null || !Bt.PreviousRunningSet.Contains(this))
            {
                OnStopRunning();
            }

            return status;
        }
        public Node(String name, CompositeNode parent = null)
        {
            this.name = name;
            this.parent = parent;
        }
    }
    public class CompositeNode : Node
    {
        private Node[] children;
        public CompositeNode(String name, Node[] children) : base(name)
        {
            this.children = children;
            foreach (Node child in children)
            {
                child.Parent = this;
            }
        }
        public Node[] Children { get => children; }
    }
    public class Sequence : CompositeNode
    {
        public Sequence(String name, Node[] children) : base(name, children) { }
        public override TaskStatus OnUpdate()
        {
            foreach (Node child in Children)
            {
                TaskStatus status = child.Tick();
                if (status != TaskStatus.Success)
                {
                    return status;
                }
            }
            return TaskStatus.Success;
        }
    }
    public class Selector : CompositeNode
    {
        public Selector(String name, Node[] children) : base(name, children) { }
        public override TaskStatus OnUpdate()
        {
            foreach (Node child in Children)
            {
                TaskStatus status = child.Tick();
                if (status != TaskStatus.Failure)
                {
                    return status;
                }
            }
            return TaskStatus.Failure;
        }
    }
    public class ExecutionNode : Node
    {
        public ExecutionNode(String name) : base(name) { }
    }
    public class Action : ExecutionNode
    {
        public Action(String name) : base(name) { }
    }
    public class Do : Action
    {
        private Func<TaskStatus> payload;
        public Do(String name, Func<TaskStatus> payload) : base(name)
        {
            this.payload = payload;
        }
        public override TaskStatus OnUpdate()
        {
            return payload();
        }
    }
    public class Condition : ExecutionNode
    {
        public Condition(String name) : base(name) { }
    }
    public class PredicateCondition : Condition
    {
        private Func<bool> predicate;
        public PredicateCondition(String name, Func<bool> predicate) : base(name)
        {
            this.predicate = predicate;
        }
        public override TaskStatus OnUpdate()
        {
            return predicate() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
    public class CBFCondition : Condition
    {
        private CBFApplicator cbfApplicator;
        public CBFCondition(String name, CBFApplicator cbfApplicator) : base(name)
        {
            this.cbfApplicator = cbfApplicator;
        }
        public override TaskStatus OnUpdate()
        {
            return cbfApplicator.isSafe() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
    public class LearningAction : Action
    {
        public LearningAction(String name, BaseAgent agent) : base(name) { this.agent = agent; }
        private BaseAgent agent;
        private int stepsPerDecision = 10;
        private int stepCount = 0;
        private int maxSteps = 1000;

        public BaseAgent Agent { get => agent; set => agent = value; }
        public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
        public int MaxSteps { get => maxSteps; set => maxSteps = value; }

        public override TaskStatus OnUpdate()
        {
            // Debug.Log(Name + ": OnUpdate");
            if (stepCount % StepsPerDecision == 0)
            {
                Agent.RequestDecision();
            }
            else
            {
                Agent.RequestAction();
            }
            stepCount++;

            if (stepCount >= MaxSteps)
            {
                Agent.EpisodeInterrupted();
                Agent.ResetEnv();
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }

        public override void OnStartRunning()
        {
            base.OnStartRunning();
            stepCount = 0;
        }
        public override void OnStopRunning()
        {
            Debug.Log(Name + ": stopped running after step: " + stepCount);
            Agent.EndEpisode();
            base.OnStopRunning();
        }
    }

    public class LearningActionWPC : LearningAction
    {
        private Func<bool> postcondition;
        public LearningActionWPC(String name, BaseAgent agent, Func<bool> postcondition) : base(name, agent)
        {
            this.postcondition = postcondition;
        }
        public override void OnStopRunning()
        {
            if (postcondition())
            {
                Agent.AddReward(1f);
                Debug.Log(Name + " reached postcondition");
            }
            else
            {
                Agent.AddReward(-1f);
                Debug.Log(Name + " did not reach postcondition");
            }
            base.OnStopRunning();
        }
    }
    public class LearningActionWPCACC : LearningActionWPC
    {
        private Func<bool>[] accs;
        public LearningActionWPCACC(string name, BaseAgent agent, Func<bool> postcondition, Func<bool>[] accs) : base(name, agent, postcondition)
        {
            this.accs = accs;
        }
        public override void OnStopRunning()
        {
            foreach (var acc in accs)
            {
                if (!acc())
                {
                    Agent.AddReward(-1f);
                    Debug.Log(Name + " violated ACC");
                }
            }
            base.OnStopRunning();
        }
    }
    public class LearningCompositeNode : CompositeNode
    {
        // TODO
        public LearningCompositeNode(String name, Node[] children) : base(name, children)
        {
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
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

}
