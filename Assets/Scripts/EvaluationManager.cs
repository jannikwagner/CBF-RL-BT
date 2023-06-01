using System;
using System.Collections.Generic;
using BTTest;
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
    public int run;
    public int globalStep;
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

public abstract class ActionEvent : Event
{
    public string agent;
    public string action;
    public float reward;
    public int localStep;
}
public abstract class ActionTerminationEvent : ActionEvent { }
public abstract class GlobalEvent : Event { }
public abstract class GlobalTerminationEvent : GlobalEvent { }

public class PostConditionReachedEvent : ActionTerminationEvent { public string postCondition; public EventType type = EventType.PostConditionReached; }
public class ACCViolatedEvent : ActionTerminationEvent { public string acc; public EventType type = EventType.ACCViolated; }
public class LocalResetEvent : ActionTerminationEvent { public EventType type = EventType.LocalReset; }
public class GlobalResetEvent : GlobalTerminationEvent { public EventType type = EventType.GlobalReset; }
public class GlobalSuccessEvent : GlobalTerminationEvent { public EventType type = EventType.GlobalSuccess; }
// public class PreConditionViolatedEvent : Event { public Condition precondition; }

public interface IEvaluationManager
{
    void AddEvent(Event ev);
    public List<Event> Events { get; }
    public void Init(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents, IEnumerable<LearningActionAgentSwitcher> actions);
}

public class EvaluationManager : IEvaluationManager
{
    private ILogDataProvider logDataProvider;
    private List<Event> events;
    private List<Event> currentRunEvents;
    private IEnumerable<Condition> conditions;
    private IEnumerable<BaseAgent> agents;
    private IEnumerable<LearningActionAgentSwitcher> actions;
    private int currentRun;

    public List<Event> Events { get => events; }
    public ILogDataProvider LogDataProvider { get => logDataProvider; set => logDataProvider = value; }

    public EvaluationManager()
    {
        events = new List<Event>();
        currentRunEvents = new List<Event>();
        currentRun = -1;
    }

    public void Init(ILogDataProvider logDataProvider, IEnumerable<Condition> conditions, IEnumerable<BaseAgent> agents, IEnumerable<LearningActionAgentSwitcher> actions)
    {
        this.logDataProvider = logDataProvider;
        this.conditions = conditions;
        this.agents = agents;
        this.actions = actions;
    }

    public void AddEvent(Event _event)
    {
        AugmentEvent(_event);
        if (currentRun != _event.run)
        {
            currentRun = _event.run;
            this.EvaluateCurrentEpisode();
            currentRunEvents.Clear();
        }
        Debug.Log(JsonUtility.ToJson(_event));
        events.Add(_event);
        currentRunEvents.Add(_event);
    }

    private void EvaluateCurrentEpisode()
    {
        var runEvaluator = new RunEvaluator(actions);

        var runStatistics = runEvaluator.EvaluateRun(currentRunEvents);
        Debug.Log(JsonUtility.ToJson(runStatistics));
    }

    private void AugmentEvent(Event _event)
    {
        _event.run = logDataProvider.Episode;
        _event.globalStep = logDataProvider.Step;
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

public class RunStatistics
{
    public bool globalSuccess = false;
    public int steps = -1;
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ActionStatistics> actionStatistics = new Dictionary<string, ActionStatistics>();
    public RunStatistics(IEnumerable<BTTest.LearningActionAgentSwitcher> actions)
    {
        foreach (BTTest.LearningActionAgentSwitcher action in actions)
        {
            actionStatistics.Add(action.Name, new ActionStatistics(action));
        }
    }
}

public class ActionStatistics
{
    public string actionName;
    public int episodes = 0;
    public List<int> steps = new List<int>();
    public List<float> rewards = new List<float>();
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ACCViolatedStatistics> accViolatedStatistics = new Dictionary<string, ACCViolatedStatistics>();

    public ActionStatistics(LearningActionAgentSwitcher action)
    {
        actionName = action.Name;
        if (action.accs != null)
        {
            foreach (var acc in action.accs)
            {
                accViolatedStatistics.Add(acc.Name, new ACCViolatedStatistics { accName = acc.Name });
            }
        }
    }
}

public class ACCViolatedStatistics
{
    public string accName;
    public int count = 0;
    public List<int> stepsToRecover = new List<int>();
    public List<bool> recovered = new List<bool>();
}

public class RunEvaluator
{
    private IEnumerable<LearningActionAgentSwitcher> actions;
    public RunEvaluator(IEnumerable<LearningActionAgentSwitcher> actions)
    {
        this.actions = actions;
    }
    public RunStatistics EvaluateRun(List<Event> runEvents)
    {
        var episodeStatistics = new RunStatistics(actions);
        var accViolationStepTemp = new Dictionary<string, Tuple<string, int>>();
        foreach (Event _event in runEvents)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                episodeStatistics.actionStatistics[actionTerminationEvent.action].steps.Add(actionTerminationEvent.localStep);
                episodeStatistics.actionStatistics[actionTerminationEvent.action].rewards.Add(actionTerminationEvent.reward);

                if (accViolationStepTemp.ContainsKey((actionTerminationEvent.action)))
                {
                    var acc = accViolationStepTemp[actionTerminationEvent.action].Item1;
                    var step = accViolationStepTemp[actionTerminationEvent.action].Item2;
                    accViolationStepTemp.Remove(actionTerminationEvent.action);
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[acc].recovered.Add(true);
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[acc].stepsToRecover.Add(actionTerminationEvent.globalStep - actionTerminationEvent.localStep - step);
                }

                if (_event is PostConditionReachedEvent)
                {
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].postConditionReachedCount++;
                    episodeStatistics.postConditionReachedCount++;
                }

                else if (_event is ACCViolatedEvent)
                {
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].accViolatedCount++;
                    episodeStatistics.accViolatedCount++;
                    var accViolatedEvent = _event as ACCViolatedEvent;
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[accViolatedEvent.acc].count++;
                    accViolationStepTemp.Add(actionTerminationEvent.action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.globalStep));
                }

                else if (_event is LocalResetEvent)
                {
                    episodeStatistics.actionStatistics[actionTerminationEvent.action].localResetCount++;
                    episodeStatistics.localResetCount++;
                }
            }
            else if (_event is GlobalTerminationEvent)
            {
                var globalTerminationEvent = _event as GlobalTerminationEvent;
                episodeStatistics.globalSuccess = globalTerminationEvent is GlobalSuccessEvent;
                episodeStatistics.steps = globalTerminationEvent.globalStep;

                foreach (var action in accViolationStepTemp.Keys)
                {
                    var acc = accViolationStepTemp[action].Item1;
                    var step = accViolationStepTemp[action].Item2;
                    episodeStatistics.actionStatistics[action].accViolatedStatistics[acc].recovered.Add(false);
                    episodeStatistics.actionStatistics[action].accViolatedStatistics[acc].stepsToRecover.Add(globalTerminationEvent.globalStep - step);
                    accViolationStepTemp.Remove(action);
                }
            }
        }
        return episodeStatistics;
    }
}
