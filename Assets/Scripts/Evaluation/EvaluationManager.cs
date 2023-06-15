using System.Collections.Generic;
using BTTest;
using UnityEngine;

public interface IEvaluationManager
{
    void AddEvent(Event _event);
    public List<Event> Events { get; }
    public void Init(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents, IEnumerable<LearningActionAgentSwitcher> actions, string runId);
}

public class EvaluationManager : IEvaluationManager
{
    private ILogDataProvider logDataProvider;
    private List<Event> events;
    private List<Event> currentCompositeEpisodeEvents;
    private IEnumerable<Condition> conditions;
    private IEnumerable<BaseAgent> agents;
    private IEnumerable<LearningActionAgentSwitcher> actions;
    private List<CompositeEpisodeStatistic> statistics = new List<CompositeEpisodeStatistic>();
    private string runId;
    private string folderPath;
    private IStorageManager storageManager;

    public List<Event> Events { get => events; }
    public ILogDataProvider LogDataProvider { get => logDataProvider; set => logDataProvider = value; }

    public EvaluationManager()
    {
        events = new List<Event>();
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
    }

    public void AddEvent(Event _event)
    {
        AugmentEvent(_event);
        Debug.Log(JsonUtility.ToJson(_event));
        events.Add(_event);
        currentCompositeEpisodeEvents.Add(_event);
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
        this.statistics.Add(statistic);
        this.storageManager.AddStatistic(statistic);
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
