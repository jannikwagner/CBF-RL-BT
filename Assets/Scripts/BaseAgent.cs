using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public abstract class BaseAgent : Agent
{
    private const float PC_REWARD = 1f;
    private const float ACC_REWARD = -1f;
    private const float HPC_REWARD = 1f;
    private const float TOTAL_TIME_PENALTY = -1f;
    private const float LOCAL_RESET_REWARD = -1f;
    public IEvaluationManager evaluationManager;
    [HideInInspector]
    public bool useCBF = true;
    private int actionCount;
    [HideInInspector]
    public int maxActions;
    private int actionsPerDecision = 5;
    private Condition postCondition;
    private IEnumerable<Condition> accs;
    private IEnumerable<Condition> higherPostConditions;
    private IEnumerable<CBFApplicator> cbfApplicators;
    protected CBFDiscreteInvalidActionMasker masker = new CBFDiscreteInvalidActionMasker();
    internal AgentSwitchingAsserter swtichingAsserter;

    public int ActionCount { get => actionCount; }
    public int MaxActions { get => maxActions; }
    public Condition PostCondition { get => postCondition; set => postCondition = value; }
    public IEnumerable<Condition> ACCs { get => accs; set => accs = value; }
    public abstract int NumActions { get; }
    public IEnumerable<CBFApplicator> CBFApplicators { get => cbfApplicators; set => cbfApplicators = value; }
    public int ActionsPerDecision { get => actionsPerDecision; set => actionsPerDecision = value; }
    public IEnumerable<Condition> HigherPostConditions { get => higherPostConditions; set => higherPostConditions = value; }

    public virtual void ResetEnvLocal() { }
    public virtual void ResetEnvGlobal() { }
    public override void OnEpisodeBegin()
    {
        // base.OnEpisodeBegin();
        Debug.Log(this + ": OnEpisodeBegin");
        swtichingAsserter.log(this, AgentSwitchingAsserter.AgentEvents.EpisodeBegin);
        actionCount = 0;
        evaluationManager.AddEvent(new ActionStartEvent { localStep = actionCount });
    }

    public void Act()
    {
        if (actionCount % actionsPerDecision == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }
    }

    public bool EpisodeShouldEnd()
    {
        return actionCount == maxActions;
    }

    public bool CheckPostCondition()
    {
        bool postconditionReached = PostCondition != null && PostCondition.Func();
        if (postconditionReached)
        {
            OnPCReached(PostCondition);
            AddReward(PC_REWARD);
            evaluationManager.AddEvent(new PostConditionReachedEvent { postCondition = PostCondition.Name, localStep = actionCount });
            Debug.Log(this + ": PostCondition " + PostCondition.Name + " reached");
        }
        return postconditionReached;
    }
    public bool CheckACCs()
    {
        bool punished = false;
        if (ACCs != null)
        {
            foreach (var acc in ACCs)
            {
                if (!acc.Func())
                {
                    OnACCViolation(acc);
                    // only for the first violated acc should be reward given and event logged
                    if (!punished)
                    {
                        AddReward(ACC_REWARD);
                        evaluationManager.AddEvent(new ACCViolatedEvent { acc = acc.Name, localStep = actionCount });
                    }
                    punished = true;
                    Debug.Log(this + ": ACC " + acc.Name + " violated");
                }
            }
        }
        return punished;
    }
    public bool CheckHigherPostConditions()
    {
        bool reached = false;
        if (HigherPostConditions != null)
        {
            foreach (var hpc in HigherPostConditions)
            {
                if (hpc.Func())
                {
                    OnHPCReached(hpc);
                    // only for the first reached hpc should be reward given and event logged
                    if (!reached)
                    {
                        AddReward(HPC_REWARD);  // probably should not give reward
                        evaluationManager.AddEvent(new HigherPostConditionReachedEvent { postCondition = hpc.Name, localStep = actionCount });
                    }
                    reached = true;
                    Debug.Log(this + ": HPC " + hpc.Name + " reached");
                }
            }
        }
        return reached;
    }

    protected virtual void OnPCReached(Condition pc) { }
    protected virtual void OnHPCReached(Condition hpc) { }
    protected virtual void OnACCViolation(Condition acc) { }
    protected abstract void ApplyTaskSpecificReward();
    protected abstract void ApplyAction(ActionBuffers actions);

    public override void OnActionReceived(ActionBuffers actions)
    {
        // base.OnActionReceived(actions);  // is this needed?
        swtichingAsserter.log(this, AgentSwitchingAsserter.AgentEvents.OnActionReceived);
        ApplyAction(actions);
        actionCount++;
        // time penalty
        AddReward(TOTAL_TIME_PENALTY / maxActions);
        ApplyTaskSpecificReward();
        bool done = CheckPostCondition();
        if (!done)
        {
            done = CheckHigherPostConditions();
        }
        if (!done)
        {
            done = CheckACCs();
        }
        if (!done && EpisodeShouldEnd())
        {
            AddReward(LOCAL_RESET_REWARD);
            Debug.Log(this + "EpisodeShouldEnd, negative reward");
            evaluationManager.AddEvent(new LocalResetEvent { localStep = actionCount });
            done = true;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!useCBF || cbfApplicators == null)
        {
            return;
        }
        if (masker == null)
        {
            masker = new CBFDiscreteInvalidActionMasker();
        }
        masker.WriteDiscreteActionMask(actionMask, cbfApplicators, NumActions);
    }
}
