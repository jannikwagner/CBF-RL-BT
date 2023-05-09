using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public abstract class BaseAgent : Agent
{
    private int actionCount;
    private int maxActions = 5000;
    private int actionsPerDecision = 10;
    private Condition postCondition;
    private List<Condition> accs;
    private List<CBFApplicator> cbfApplicators;
    private CBFDiscreteInvalidActionMasker masker;

    // public int ActionCount { get => actionCount; set => actionCount = value; }
    // public int MaxActions { get => maxActions; set => maxActions = value; }
    // public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
    public Condition PostCondition { get => postCondition; set => postCondition = value; }
    public List<Condition> ACCs { get => accs; set => accs = value; }
    public abstract int NumActions { get; }
    public List<CBFApplicator> CBFApplicators { get => cbfApplicators; set => cbfApplicators = value; }
    public int ActionsPerDecision { get => actionsPerDecision; set => actionsPerDecision = value; }

    public virtual void ResetEnvLocal() { }
    public virtual void ResetEnvGlobal() { }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        actionCount = 0;
        Debug.Log(this + "OnEpisodeBegin");
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

    public void ApplyPostConditionReward()
    {
        if (PostCondition != null && PostCondition.Func())
        {
            AddReward(1f);
            Debug.Log("PostCondition " + PostCondition.Name + " met");
        }
    }
    public void ApplyACCReward()
    {
        bool violated = false;
        if (ACCs != null)
        {
            foreach (var acc in ACCs)
            {
                if (!acc.Func())
                {
                    OnACCViolation();
                    Debug.Log("ACC " + acc.Name + " violated");
                    violated = true;
                }
            }
            if (violated)
            {
                AddReward(-1f);
            }
        }
    }

    protected abstract void OnACCViolation();

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionCount++;
        base.OnActionReceived(actions);
        AddReward(-1f / maxActions);
        ApplyPostConditionReward();
        ApplyACCReward();
        if (EpisodeShouldEnd())
        {
            AddReward(-1f);
            Debug.Log(this + "EpisodeShouldEnd, negative reward");
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (cbfApplicators == null)
        {
            return;
        }
        if (masker == null)
        {
            masker = new CBFDiscreteInvalidActionMasker();
        }
        masker.WriteDiscreteActionMask(actionMask, cbfApplicators.ToArray(), NumActions);
    }
}
