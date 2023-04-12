using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;

namespace BTTest
{
    public enum ReturnStatus { SUCCESS, FAILURE, RUNNING };

    public class BT
    {
        private Node root;
        private HashSet<Node> currentExecutionSet;
        private HashSet<Node> previousExecutionSet;
        private HashSet<Node> currentRunningSet;
        private HashSet<Node> previousRunningSet;
        public Node Root { get => root; set => SetReferenceRec(value); }
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

        public virtual ReturnStatus OnUpdate() { return ReturnStatus.FAILURE; }
        public virtual void OnStopRunning() { }
        public virtual void OnStartRunning() { }
        public virtual void OnStopExecution() { }
        public virtual void OnSartExecution() { }
        public virtual void OnInit() { }
        public virtual ReturnStatus Tick()
        {
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

            ReturnStatus status = OnUpdate();

            if (status == ReturnStatus.RUNNING)
            {
                Bt.CurrentRunningSet.Add(this);
            }
            else if (Bt.PreviousRunningSet != null && !Bt.PreviousRunningSet.Contains(this))
            {
                OnStopRunning();
            }

            return status;
        }
        public Node(String name, Node parent = null)
        {
            this.name = name;
            this.parent = parent;
        }
    }
    public class CompositeNode : Node
    {
        private Node[] children;
        public CompositeNode(String name, Node[] children, Node parent = null) : base(name, parent)
        {
            this.Children = children;
            foreach (Node child in children)
            {
                child.Parent = this;
            }
        }

        public Node[] Children { get => children; set => children = value; }
    }
    public class Sequence : CompositeNode
    {
        public Sequence(String name, Node[] children, Node parent = null) : base(name, children, parent)
        {
        }
        public override ReturnStatus OnUpdate()
        {
            foreach (Node child in Children)
            {
                ReturnStatus status = child.Tick();
                if (status != ReturnStatus.SUCCESS)
                {
                    return status;
                }
            }
            return ReturnStatus.SUCCESS;
        }
    }
    public class Selector : CompositeNode
    {
        public Selector(String name, Node[] children, Node parent = null) : base(name, children, parent)
        {
        }
        public override ReturnStatus OnUpdate()
        {
            foreach (Node child in Children)
            {
                ReturnStatus status = child.Tick();
                if (status != ReturnStatus.FAILURE)
                {
                    return status;
                }
            }
            return ReturnStatus.FAILURE;
        }
    }
    public class ExecutionNode : Node
    {
        public ExecutionNode(String name, Node parent = null) : base(name, parent)
        {
        }
    }
    public class Action : ExecutionNode
    {
        public Action(String name, Node parent = null) : base(name, parent)
        {
        }
    }
    public class Condition : ExecutionNode
    {
        public Condition(String name, Node parent = null) : base(name, parent)
        {
        }
    }
    public class CBFCondition : Condition
    {
        private CBFApplicator cbfApplicator;
        public CBFCondition(String name, CBFApplicator cbfApplicator, Node parent = null) : base(name, parent)
        {
            this.cbfApplicator = cbfApplicator;
        }
        public override ReturnStatus OnUpdate()
        {
            return cbfApplicator.isSafe() ? ReturnStatus.SUCCESS : ReturnStatus.FAILURE;
        }
    }
    public class LearningAction : Action
    {
        // TODO
        private BehaviorParameters behaviorParameters;
        private IActuator actuator;
        private ISensor sensor;
        public LearningAction(String name, Node parent = null) : base(name, parent)
        {
        }
        public override ReturnStatus OnUpdate()
        {
            return ReturnStatus.FAILURE;
        }
    }
    public class LearningCompositeNode : CompositeNode
    {
        // TODO
        private BehaviorParameters behaviorParameters;
        private IActuator actuator;
        private ISensor sensor;
        public LearningCompositeNode(String name, Node[] children, Node parent = null) : base(name, children, parent)
        {
        }
        public override ReturnStatus OnUpdate()
        {
            return ReturnStatus.FAILURE;
        }
    }
}
