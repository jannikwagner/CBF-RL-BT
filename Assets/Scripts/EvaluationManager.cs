using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStepCounter
{
    int Step { get; }
}
public interface IEpisodeCounter
{
    int Episode { get; }
}
public interface IActionProvider
{
    BTTest.Action Action { get; }
}
public interface ILogDataProvider : IStepCounter, IEpisodeCounter, IAgentProvider, IActionProvider { }

public abstract class Event
{
    public int btEpisode;
    public int step;
    public string agent;
    public string action;
    public float reward;
    // void FromJSON(string json);
    // string ToJSON();
}

public enum EventType
{
    PostConditionReached,
    ACCViolated,
    LocalReset,
    GlobalReset,
    GlobalSuccess,
    // PreConditionViolated,
}

public class PostConditionReachedEvent : Event { public string postCondition; public EventType type = EventType.PostConditionReached; }
public class ACCViolatedEvent : Event { public string acc; public EventType type = EventType.ACCViolated; }
public class LocalResetEvent : Event { public EventType type = EventType.LocalReset; }
public class GlobalResetEvent : Event { public EventType type = EventType.GlobalReset; }
public class GlobalSuccessEvent : Event { public EventType type = EventType.GlobalSuccess; }
// public class PreConditionViolatedEvent : Event { public Condition precondition; }

public interface IEvaluationManager
{
    void AddEvent(Event ev);
    public List<Event> Events { get; }
}

public class EvaluationManager : IEvaluationManager
{
    private ILogDataProvider logDataProvider;
    private List<Event> events;
    private List<Event> currentRunEvents;
    private IEnumerable<Condition> conditions;
    private IEnumerable<BaseAgent> agents;
    private int currentRun;

    public List<Event> Events { get => events; }
    public ILogDataProvider LogDataProvider { get => logDataProvider; set => logDataProvider = value; }

    public EvaluationManager(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents)
    {
        this.logDataProvider = logDataProvider;
        this.conditions = conditions;
        this.agents = agents;
        events = new List<Event>();
        currentRunEvents = new List<Event>();
        currentRun = -1;
    }

    public void AddEvent(Event _event)
    {
        AugmentEvent(_event);
        if (currentRun != _event.btEpisode)
        {
            currentRun = _event.btEpisode;
            this.EvaluateCurrentEpisode();
            currentRunEvents.Clear();
        }
        Debug.Log(JsonUtility.ToJson(_event));
        events.Add(_event);
        currentRunEvents.Add(_event);
    }

    private void EvaluateCurrentEpisode()
    {
        var runEvaluator = new RunEvaluator();

        var runStatistics = runEvaluator.EvaluateRun(currentRunEvents);
        Debug.Log(JsonUtility.ToJson(runStatistics));
    }

    private void AugmentEvent(Event _event)
    {
        _event.btEpisode = logDataProvider.Episode;
        _event.step = logDataProvider.Step;
        _event.agent = logDataProvider.Agent.name;
        _event.action = logDataProvider.Action.Name;
        _event.reward = logDataProvider.Agent.GetCumulativeReward();
    }
}

public class RunStatistics
{
    public bool globalSuccess;
    public int steps;
    public Dictionary<string, int> numEpisodsesPerAction;
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
}

public class RunEvaluator
{
    public RunStatistics EvaluateRun(List<Event> runEvents)
    {
        var episodeStatistics = new RunStatistics();
        foreach (Event _event in runEvents)
        {
            switch (_event)
            {
                case PostConditionReachedEvent postConditionReachedEvent:
                    episodeStatistics.postConditionReachedCount++;
                    break;
                case ACCViolatedEvent accViolatedEvent:
                    episodeStatistics.accViolatedCount++;
                    break;
                case LocalResetEvent localResetEvent:
                    episodeStatistics.localResetCount++;
                    break;
                case GlobalResetEvent globalResetEvent:
                    episodeStatistics.globalSuccess = false;
                    episodeStatistics.steps = globalResetEvent.step;
                    break;
                case GlobalSuccessEvent globalSuccessEvent:
                    episodeStatistics.globalSuccess = true;
                    episodeStatistics.steps = globalSuccessEvent.step;
                    break;
            }
        }
        return episodeStatistics;
    }
}
