using Unity.MLAgents;
using UnityEngine;

public class BaseAgent : Agent
{
    private int actionCount;
    private int maxActions;
    private int stepsPerDecision;

    public int ActionCount { get => actionCount; set => actionCount = value; }
    public int MaxActions { get => maxActions; set => maxActions = value; }
    public int StepsPerDecision { get => stepsPerDecision; set => stepsPerDecision = value; }

    public virtual void ResetEnv() { }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        actionCount = 0;
        Debug.Log("OnEpisodeBegin");
    }

    public void Act()
    {
        if (actionCount % stepsPerDecision == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }
        actionCount++;
    }

    public bool EpisodeShouldEnd()
    {
        return actionCount == maxActions;
    }
}
