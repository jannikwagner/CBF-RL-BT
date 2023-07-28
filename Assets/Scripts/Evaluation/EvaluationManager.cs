using System.Collections.Generic;
using BTTest;
using UnityEngine;

public interface IEvaluationManager
{
    void AddEvent(Event _event);
    public void Init(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents, IEnumerable<LearningActionAgentSwitcher> actions, string runId);
}

public class EvaluationManager : IEvaluationManager
{
    private ILogDataProvider logDataProvider;
    private List<Event> currentCompositeEpisodeEvents;
    private IEnumerable<Condition> conditions;
    private IEnumerable<BaseAgent> agents;
    private IEnumerable<LearningActionAgentSwitcher> actions;
    private string runId;
    private string folderPath;
    private IStorageManager storageManager;
    private bool isActive = false;

    public EvaluationManager()
    {
        currentCompositeEpisodeEvents = new List<Event>();
    }

    public void Init(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents, IEnumerable<LearningActionAgentSwitcher> actions, string runId)
    {
        this.logDataProvider = logDataProvider;
        this.conditions = conditions;
        this.agents = agents;
        this.actions = actions;
        this.runId = runId;
        folderPath = $@"evaluation/stats/{runId}";
        storageManager = new StorageManager(folderPath);
        isActive = true;
    }

    public void AddEvent(Event _event)
    {
        if (!isActive)
        {
            return;
        }
        AugmentEvent(_event);
        Debug.Log(JsonUtility.ToJson(_event));
        currentCompositeEpisodeEvents.Add(_event);
        // TODO: should maybe be a method and not based on a specific type of event
        if (_event is GlobalTerminationEvent)
        {
            this.EvaluateCurrentEpisode();
            currentCompositeEpisodeEvents.Clear();
        }
    }

    private void EvaluateCurrentEpisode()
    {
        var compositeEpisodeEvaluator = new CompositeEpisodeEvaluator(actions);

        var statistic = compositeEpisodeEvaluator.EvaluateCompositeEpisode(currentCompositeEpisodeEvents);
        if (statistic != null)
        {
            this.storageManager.AddStatistic(statistic);
        }
    }

    private void AugmentEvent(Event _event)
    {
        _event.compositeEpisodeNumber = logDataProvider.Episode;
        _event.btStep = logDataProvider.Step;
        if (_event is ActionEvent)
        {
            AugmentActionEvent(_event as ActionEvent);
        }
    }

    private void AugmentActionEvent(ActionEvent actionEvent)
    {
        actionEvent.agent = logDataProvider.Agent.name;
        actionEvent.action = logDataProvider.Action.Name;
        actionEvent.reward = logDataProvider.Agent.GetCumulativeReward();
    }
}
