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

public abstract class Event
{
    public int episode;
    public int step;
    public string agent;
    public abstract EventType type { get; }
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

public class PostConditionReachedEvent : Event { public string postCondition; public override EventType type => EventType.PostConditionReached; }
public class ACCViolatedEvent : Event { public string acc; public override EventType type => EventType.ACCViolated; }
public class LocalResetEvent : Event { public override EventType type => EventType.LocalReset; }
public class GlobalResetEvent : Event { public override EventType type => EventType.GlobalReset; }
public class GlobalSuccessEvent : Event { public override EventType type => EventType.GlobalSuccess; }
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
        _event.agent = logDataProvider.Agent.name;
        string jsonString = JsonUtility.ToJson(_event);
        Debug.Log(jsonString);
        events.Add(_event);
    }
}
