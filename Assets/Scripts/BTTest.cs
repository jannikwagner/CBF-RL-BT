using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

namespace BTTest
{
    public enum ReturnStatus { SUCCESS, FAILURE, RUNNING };

    public class Node
    {
        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private Node parent;
        public Node Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        public virtual ReturnStatus Tick() { return ReturnStatus.FAILURE; }
        public Node(String name, Node parent = null)
        {
            this.name = name;
            this.parent = parent;
        }
    }
    public class CompositeNode : Node
    {
        public Node[] children;
        public CompositeNode(String name, Node[] children, Node parent = null) : base(name, parent)
        {
            this.children = children;
            foreach (Node child in children)
            {
                child.Parent = this;
            }
        }
    }
    public class Sequence : CompositeNode
    {
        public Sequence(String name, Node[] children, Node parent = null) : base(name, children, parent)
        {
        }
        public override ReturnStatus Tick()
        {
            foreach (Node child in children)
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
        public override ReturnStatus Tick()
        {
            foreach (Node child in children)
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
        public override ReturnStatus Tick()
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
        public override ReturnStatus Tick()
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
        public override ReturnStatus Tick()
        {
            return ReturnStatus.FAILURE;
        }
    }
}
