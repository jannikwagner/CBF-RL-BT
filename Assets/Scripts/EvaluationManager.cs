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
public interface ILogDataProvider : IStepCounter, IEpisodeCounter, IAgentProvider { }

public class Event
{
    public int episode;
    public int step;
    public BaseAgent agent;
    // void FromJSON(string json);
    // string ToJSON();
}

public class PostConditionReachedEvent : Event { public Condition postCondition; }
// public class PreConditionViolatedEvent : Event { public Condition precondition; }
public class ACCViolatedEvent : Event { public Condition acc; }
public class LocalResetEvent : Event { }
public class GlobalResetEvent : Event { }
public class GlobalSuccessEvent : Event { }

public interface IEvaluationManager
{
    void AddEvent(Event ev);
    public List<Event> Events { get; }
}

public class EvaluationManager : IEvaluationManager
{
    private ILogDataProvider logDataProvider;
    private List<Event> events;

    public List<Event> Events { get => events; }
    public ILogDataProvider LogDataProvider { get => logDataProvider; set => logDataProvider = value; }

    public EvaluationManager(ILogDataProvider logDataProvider)
    {
        this.logDataProvider = logDataProvider;
        events = new List<Event>();
    }

    public void AddEvent(Event _event)
    {
        _event.episode = logDataProvider.Episode;
        _event.step = logDataProvider.Step;
        _event.agent = logDataProvider.Agent;
        string jsonString = JsonUtility.ToJson(_event);
        Debug.Log(jsonString);
        events.Add(_event);
    }
}
