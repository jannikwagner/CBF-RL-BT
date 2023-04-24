using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class BaseAgent : Agent
{
    private int actionCount;
    private int maxActions = 500;
    private int stepsPerDecision = 5;
    private Func<bool> postCondition;
    private List<Func<bool>> accs;

    // public int ActionCount { get => actionCount; set => actionCount = value; }
    // public int MaxActions { get => maxActions; set => maxActions = value; }
    // public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }
    public Func<bool> PostCondition { get => postCondition; set => postCondition = value; }
    public List<Func<bool>> ACCs { get => accs; set => accs = value; }

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
        if (actionCount++ % stepsPerDecision == 0)
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
        if (PostCondition != null && PostCondition())
        {
            AddReward(1f);
            Debug.Log("PostCondition met");
        }
    }
    public void ApplyACCReward()
    {
        if (ACCs != null && !ACCs.TrueForAll(x => x()))
        {
            AddReward(-1f);
            Debug.Log("ACC violated");
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
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
}
