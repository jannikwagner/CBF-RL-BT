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

public enum ActionTerminationCause
{
    PostConditionReached,
    ACCViolated,
    LocalReset,
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
    void AddEvent(Event _event);
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
    private List<RunStatistics> runStatistics = new List<RunStatistics>();

    public List<Event> Events { get => events; }
    public ILogDataProvider LogDataProvider { get => logDataProvider; set => logDataProvider = value; }

    public EvaluationManager()
    {
        events = new List<Event>();
        currentRunEvents = new List<Event>();
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
        Debug.Log(JsonUtility.ToJson(_event));
        events.Add(_event);
        currentRunEvents.Add(_event);
        if (_event is GlobalTerminationEvent)
        {
            this.EvaluateCurrentEpisode();
            currentRunEvents.Clear();
        }
    }

    private void EvaluateCurrentEpisode()
    {
        var runEvaluator = new RunEvaluator(actions);

        var runStatistics = runEvaluator.EvaluateRun(currentRunEvents);
        this.runStatistics.Add(runStatistics);
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
    public int episodeCount = 0;
    public List<EpisodeStatistics> episodes = new List<EpisodeStatistics>();
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

public class EpisodeStatistics
{
    public int steps = 0;
    public float reward = 0;
    public ActionTerminationCause cause;
    public string accName;
    public int accStepsToRecovery;
    public bool accRecovered;
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
        var runStatistics = new RunStatistics(actions);
        var accViolationStepTemp = new Dictionary<string, Tuple<string, int>>();

        foreach (Event _event in runEvents)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                var episodeStatistics = new EpisodeStatistics { steps = actionTerminationEvent.localStep, reward = actionTerminationEvent.reward };
                // runStatistics.actionStatistics[actionTerminationEvent.action].steps.Add(actionTerminationEvent.localStep);
                // runStatistics.actionStatistics[actionTerminationEvent.action].rewards.Add(actionTerminationEvent.reward);
                string action = actionTerminationEvent.action;
                runStatistics.actionStatistics[action].episodes.Add(episodeStatistics);
                runStatistics.actionStatistics[action].episodeCount += 1;

                // this action has previously violated an acc
                if (accViolationStepTemp.ContainsKey(action))
                {
                    var acc = accViolationStepTemp[action].Item1;
                    var violationGlobalStep = accViolationStepTemp[action].Item2;
                    bool successfullyRecovered = true;
                    int stepsToRecover = actionTerminationEvent.globalStep - actionTerminationEvent.localStep - violationGlobalStep;
                    accViolationStepTemp.Remove(action);

                    trackACCRecovery(runStatistics, action, acc, stepsToRecover, successfullyRecovered);
                }

                if (_event is PostConditionReachedEvent)
                {
                    runStatistics.postConditionReachedCount++;
                    runStatistics.actionStatistics[action].postConditionReachedCount++;
                    episodeStatistics.cause = ActionTerminationCause.PostConditionReached;
                }

                else if (_event is ACCViolatedEvent)
                {
                    runStatistics.accViolatedCount++;
                    runStatistics.actionStatistics[action].accViolatedCount++;
                    episodeStatistics.cause = ActionTerminationCause.ACCViolated;

                    var accViolatedEvent = _event as ACCViolatedEvent;
                    runStatistics.actionStatistics[action].accViolatedStatistics[accViolatedEvent.acc].count++;
                    // prepare evaluating recovery
                    accViolationStepTemp.Add(action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.globalStep));
                }

                else if (_event is LocalResetEvent)
                {
                    runStatistics.localResetCount++;
                    runStatistics.actionStatistics[action].localResetCount++;
                    episodeStatistics.cause = ActionTerminationCause.LocalReset;
                }
            }

            else if (_event is GlobalTerminationEvent)
            {
                var globalTerminationEvent = _event as GlobalTerminationEvent;
                runStatistics.globalSuccess = globalTerminationEvent is GlobalSuccessEvent;
                runStatistics.steps = globalTerminationEvent.globalStep;

                foreach (var action in accViolationStepTemp.Keys)
                {
                    var acc = accViolationStepTemp[action].Item1;
                    int violationGlobalStep = accViolationStepTemp[action].Item2;
                    var stepsToRecover = globalTerminationEvent.globalStep - violationGlobalStep;
                    bool successfullyRecovered = false;
                    accViolationStepTemp.Remove(action);

                    trackACCRecovery(runStatistics, action, acc, stepsToRecover, successfullyRecovered);
                }
            }
        }

        return runStatistics;
    }

    private static void trackACCRecovery(RunStatistics runStatistics, string action, string acc, int stepsToRecover, bool successfullyRecovered)
    {
        ACCViolatedStatistics aCCViolatedStatistics = runStatistics.actionStatistics[action].accViolatedStatistics[acc];
        aCCViolatedStatistics.recovered.Add(successfullyRecovered);
        aCCViolatedStatistics.stepsToRecover.Add(stepsToRecover);

        List<EpisodeStatistics> episodes = runStatistics.actionStatistics[action].episodes;
        var previousEpisodeStatistics = episodes[episodes.Count - 2];
        previousEpisodeStatistics.accName = acc;
        previousEpisodeStatistics.accStepsToRecovery = stepsToRecover;
        previousEpisodeStatistics.accRecovered = successfullyRecovered;
    }
}
