using System;
using UnityEngine;
using System.Collections.Generic;
using Env5;
using System.Linq;

namespace BTTest
{
    public enum TaskStatus { Success, Failure, Running };

    public class BT
    {
        public BT(Node root) { this.Root = root; Init(); }
        private Node root;
        private HashSet<Node> currentExecutionSet;
        private HashSet<Node> previousExecutionSet;
        private HashSet<Node> currentRunningSet;
        private HashSet<Node> previousRunningSet;
        private Action currentAction;
        private long step;
        private Node Root { get => root; set { this.root = value; SetReferenceRec(value); } }
        public HashSet<Node> CurrentExecutionSet { get => currentExecutionSet; set => currentExecutionSet = value; }
        public HashSet<Node> PreviousExecutionSet { get => previousExecutionSet; set => previousExecutionSet = value; }
        public HashSet<Node> CurrentRunningSet { get => currentRunningSet; set => currentRunningSet = value; }
        public HashSet<Node> PreviousRunningSet { get => previousRunningSet; set => previousRunningSet = value; }
        public long Step { get => step; }
        public Action CurrentAction { get => currentAction; set => currentAction = value; }

        private void Init()
        {
            currentExecutionSet = null;
            previousExecutionSet = null;
            currentRunningSet = null;
            previousRunningSet = null;
            currentAction = null;
            step = 0;
        }

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
            step++;
            return status;
        }

        public void Reset()
        {
            this.Root.OnReset();
            Init();
        }

        public List<T> FindNodes<T>(Func<T, bool> predicate = null) where T : Node
        {
            var nodes = new List<T>();
            FindNodesRec(Root, predicate, nodes);
            return nodes;
        }

        private void FindNodesRec<T>(Node root, Func<T, bool> predicate, List<T> nodes) where T : Node
        {
            if ((root is T) && (predicate == null || predicate(root as T)))
            {
                nodes.Add(root as T);
            }
            if (root is CompositeNode)
            {
                foreach (Node child in (root as CompositeNode).Children)
                {
                    FindNodesRec(child, predicate, nodes);
                }
            }
        }

        public string printTree(Node tree = null, int depth = 0)
        {
            if (tree == null)
            {
                tree = Root;
            }
            string subTree = "";
            string prefix = string.Concat(Enumerable.Repeat("| ", depth));
            if (tree is CompositeNode)
            {
                var symbol = tree is Selector ? "F" : tree is Sequence ? "S" : "X";
                subTree = subTree + prefix + symbol + "\n";
                foreach (var child in (tree as CompositeNode).Children)
                {
                    subTree = subTree + printTree(child, depth + 1);
                }
            }
            else if (tree is Action)
            {
                subTree = subTree + prefix + "[" + tree.Name + "]" + "\n";
            }
            else if (tree is ConditionNode)
            {
                subTree = subTree + prefix + "(" + tree.Name + ")" + "\n";
            }
            else
            {
                Debug.Log(tree);
                throw new Exception("Unknown node type");
            }
            return subTree;
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
        public virtual void OnReset() { Initialized = false; }
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
        private IEnumerable<Node> children;
        public CompositeNode(String name, IEnumerable<Node> children) : base(name)
        {
            this.children = children;
            foreach (Node child in children)
            {
                child.Parent = this;
            }
        }
        public IEnumerable<Node> Children { get => children; }
        public override void OnReset()
        {
            foreach (var child in children)
            {
                child.OnReset();
            }
            base.OnReset();
        }
    }
    public class Sequence : CompositeNode
    {
        public Sequence(String name, IEnumerable<Node> children) : base(name, children) { }
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
        public Selector(String name, IEnumerable<Node> children) : base(name, children) { }
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
        public override TaskStatus Tick()
        {
            this.Bt.CurrentAction = this;

            TaskStatus status = base.Tick();
            return status;
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
    public class ConditionNode : ExecutionNode
    {
        public ConditionNode(String name) : base(name) { }
    }
    public class PredicateCondition : ConditionNode
    {
        private Condition predicate;
        public PredicateCondition(String name, Condition predicate) : base(name)
        {
            this.predicate = predicate;
        }
        public override TaskStatus OnUpdate()
        {
            return predicate.Func() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
    public class CBFCondition : ConditionNode
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
    public class SwitchedLearningAction : Action
    {
        public SwitchedLearningAction(String name, BaseAgent agent, IAgentSwitcher switcher, Condition postCondition = null, IEnumerable<Condition> accs = null, IEnumerable<Condition> higherPostConditions = null, IEnumerable<CBFApplicator> cbfApplicators = null) : base(name) { this.agent = agent; this.switcher = switcher; this.postCondition = postCondition; this.accs = accs; this.higherPostConditions = higherPostConditions; this.cbfApplicators = cbfApplicators; }
        protected BaseAgent agent;
        protected IAgentSwitcher switcher;
        public Condition postCondition;
        public IEnumerable<Condition> accs;
        public IEnumerable<Condition> higherPostConditions;
        public IEnumerable<CBFApplicator> cbfApplicators;

        public override TaskStatus OnUpdate()
        {
            // Debug.Log(Name + ": OnUpdate");
            agent.PostCondition = postCondition;
            agent.ACCs = accs;
            agent.HigherPostConditions = higherPostConditions;
            agent.CBFApplicators = cbfApplicators;
            switcher.Act(agent);
            return TaskStatus.Running;
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
            Agent.AddReward(-1f / MaxSteps);
            stepCount++;

            if (stepCount >= MaxSteps)
            {
                Log("Local Reset");
                // Agent.AddReward(-1f);
                Agent.EpisodeInterrupted();
                Agent.SetReward(0);
                Agent.ResetEnvLocal();
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }
        public override void OnInit()
        {
            base.OnInit();
            Agent.gameObject.SetActive(false);
        }
        public override void OnStartRunning()
        {
            base.OnStartRunning();

            Log("OnStartRunning");

            Agent.gameObject.SetActive(true);
            Agent.SetReward(0);
            stepCount = 0;
        }
        public override void OnStopRunning()
        {
            Log("OnStopRunning");
            // Agent.EndEpisode();
            // Agent.SetReward(0);
            Agent.gameObject.SetActive(false);
            base.OnStopRunning();
        }
        public override void OnReset()
        {
            Log("OnReset");
            Agent.EpisodeInterrupted();
            Agent.SetReward(0);
            stepCount = 0;
            base.OnReset();
        }
        protected string GetLog(string message)
        {
            return "step: " + stepCount + ",\t reward: " + Utility.Round(Agent.GetCumulativeReward(), 4) + ",\t BT step: " + Bt.Step + ", \t" + Name + ": \t" + message;
        }
        protected void Log(string message)
        {
            Debug.Log(GetLog(message));
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
                Log("Reached Postcondition");
            }
            // else
            // {
            //     Agent.AddReward(-1f);
            //     Log("Did Not Reach Postcondition");
            // }
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
                    Agent.AddReward(-0.1f);
                    Log("Violated ACC");
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
