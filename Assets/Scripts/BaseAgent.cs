using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public abstract class BaseAgent : Agent
{
    public IEvaluationManager evaluationManager;
    public bool useCBF = true;
    private int actionCount;
    private int maxActions = 5000;
    private int actionsPerDecision = 5;
    private Condition postCondition;
    private IEnumerable<Condition> accs;
    private IEnumerable<CBFApplicator> cbfApplicators;
    protected CBFDiscreteInvalidActionMasker masker = new CBFDiscreteInvalidActionMasker();

    public int ActionCount { get => actionCount; }
    public int MaxActions { get => maxActions; }
    // public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
    public Condition PostCondition { get => postCondition; set => postCondition = value; }
    public IEnumerable<Condition> ACCs { get => accs; set => accs = value; }
    public abstract int NumActions { get; }
    public IEnumerable<CBFApplicator> CBFApplicators { get => cbfApplicators; set => cbfApplicators = value; }
    public int ActionsPerDecision { get => actionsPerDecision; set => actionsPerDecision = value; }

    public virtual void ResetEnvLocal() { }
    public virtual void ResetEnvGlobal() { }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        actionCount = 0;
        Debug.Log(this + ": OnEpisodeBegin");
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

    public void CheckPostCondition()
    {
        if (PostCondition != null && PostCondition.Func())
        {
            AddReward(1f);
            Debug.Log(this + ": PostCondition " + PostCondition.Name + " met");
            evaluationManager.AddEvent(new PostConditionReachedEvent { postCondition = PostCondition.Name, localStep = actionCount });
        }
    }
    public void CheckACCs()
    {
        bool punished = false;
        if (ACCs != null)
        {
            foreach (var acc in ACCs)
            {
                if (!acc.Func())
                {
                    OnACCViolation();
                    if (!punished)
                    {
                        AddReward(-1f);
                    }
                    punished = true;
                    Debug.Log(this + ": ACC " + acc.Name + " violated");
                    evaluationManager.AddEvent(new ACCViolatedEvent { acc = acc.Name, localStep = actionCount });
                }
            }
        }
    }

    protected abstract void OnACCViolation();

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionCount++;
        base.OnActionReceived(actions);
        AddReward(-1f / maxActions);
        CheckPostCondition();
        CheckACCs();
        if (EpisodeShouldEnd())
        {
            AddReward(-1f);
            Debug.Log(this + "EpisodeShouldEnd, negative reward");
            evaluationManager.AddEvent(new LocalResetEvent { localStep = actionCount });
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
