using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;

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
        public Node Root { get => root; set { this.root = value; SetReferenceRec(value); } }
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
        public void Tick()
        {
            currentRunningSet = new HashSet<Node>();
            currentExecutionSet = new HashSet<Node>();
            Root.Tick();
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
        }
    }

    public class Node
    {
        private bool initialized = false;
        private BT bt;
        private String name;
        public String Name
        {
            get => name; set => name = value;
        }
        private Node parent;
        public Node Parent
        {
            get => parent; set => parent = value;
        }
        public BT Bt { get => bt; set => bt = value; }
        public bool Initialized { get => initialized; set => initialized = value; }

        public virtual TaskStatus OnUpdate() { return TaskStatus.Failure; }
        public virtual void OnStopRunning() { }
        public virtual void OnStartRunning() { }
        public virtual void OnStopExecution() { }
        public virtual void OnSartExecution() { }
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
            if (Bt.PreviousExecutionSet != null && !Bt.PreviousExecutionSet.Contains(this))
            {
                OnSartExecution();
            }
            if (Bt.PreviousRunningSet != null && !Bt.PreviousRunningSet.Contains(this))
            {
                OnStartRunning();
            }

            TaskStatus status = OnUpdate();

            if (status == TaskStatus.Running)
            {
                Bt.CurrentRunningSet.Add(this);
            }
            // if it were in previous, it would be stopped in BT
            else if (Bt.PreviousRunningSet != null && !Bt.PreviousRunningSet.Contains(this))
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
        public Sequence(String name, Node[] children) : base(name, children)
        {
        }
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
        public Selector(String name, Node[] children) : base(name, children)
        {
        }
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
        public ExecutionNode(String name) : base(name)
        {
        }
    }
    public class Action : ExecutionNode
    {
        public Action(String name) : base(name)
        {
        }
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
    public class PredicateCondition : ExecutionNode
    {
        private Func<bool> predicate;
        public PredicateCondition(String name, Func<bool> predicate = null) : base(name)
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
        // TODO
        private Agent agent;
        public LearningAction(String name, Agent agent) : base(name)
        {
            this.agent = agent;
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
        }
    }
    public class LearningActionWPC : LearningAction
    {
        private Func<bool> postcondition;
        public LearningActionWPC(String name, Agent agent, Func<bool> postcondition) : base(name, agent)
        {
            this.postcondition = postcondition;
        }
    }
    public class LearningCompositeNode : CompositeNode
    {
        // TODO
        private BehaviorParameters behaviorParameters;
        private IActuator actuator;
        private ISensor sensor;
        public LearningCompositeNode(String name, Node[] children) : base(name, children)
        {
        }
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
        }
    }
}
