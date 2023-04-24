using Unity.MLAgents;
using UnityEngine;

public class BaseAgent : Agent
{
    private int actionCount;
    private int maxActions = 500;
    private int stepsPerDecision = 10;

    // public int ActionCount { get => actionCount; set => actionCount = value; }
    // public int MaxActions { get => maxActions; set => maxActions = value; }
    // public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }

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
}
