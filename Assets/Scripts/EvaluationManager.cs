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
    public int compositeEpisode;
    public int btStep;
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
        Debug.Log(JsonUtility.ToJson(statistic));
        Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(this.statistics));
    }

    private void AugmentEvent(Event _event)
    {
        _event.compositeEpisode = logDataProvider.Episode;
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

public class CompositeEpisodeStatistic
{
    public bool globalSuccess = false;
    public int steps = -1;
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ActionStatistic> actionStatistics = new Dictionary<string, ActionStatistic>();

    public CompositeEpisodeStatistic(IEnumerable<BTTest.LearningActionAgentSwitcher> actions)
    {
        foreach (BTTest.LearningActionAgentSwitcher action in actions)
        {
            actionStatistics.Add(action.Name, new ActionStatistic(action));
        }
    }
}

public class ActionStatistic
{
    public string actionName;
    public int episodeCount = 0;
    public List<EpisodeStatistic> episodes = new List<EpisodeStatistic>();
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ACCViolatedStatistic> accViolatedStatistics = new Dictionary<string, ACCViolatedStatistic>();

    public ActionStatistic(LearningActionAgentSwitcher action)
    {
        actionName = action.Name;
        if (action.accs != null)
        {
            foreach (var acc in action.accs)
            {
                accViolatedStatistics.Add(acc.Name, new ACCViolatedStatistic { accName = acc.Name });
            }
        }
    }
}

public class EpisodeStatistic
{
    public int steps = 0;
    public float reward = 0;
    public ActionTerminationCause cause;
    public ACCViolatedInfo accInfo = null;
}

public class ACCViolatedInfo
{
    public string accName;
    public int stepsToRecover;
    public bool recovered;
}

public class ACCViolatedStatistic
{
    public string accName;
    public int count = 0;
    public List<int> stepsToRecover = new List<int>();
    public List<bool> recovered = new List<bool>();
}

public class CompositeEpisodeEvaluator
{
    private IEnumerable<LearningActionAgentSwitcher> actions;

    public CompositeEpisodeEvaluator(IEnumerable<LearningActionAgentSwitcher> actions)
    {
        this.actions = actions;
    }

    public CompositeEpisodeStatistic EvaluateCompositeEpisode(List<Event> compositeEpisodeEvents)
    {
        var compositeEpisodeStatistics = new CompositeEpisodeStatistic(actions);
        var accViolationStepTemp = new Dictionary<string, Tuple<string, int>>();

        foreach (Event _event in compositeEpisodeEvents)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                var episodeStatistics = new EpisodeStatistic { steps = actionTerminationEvent.localStep, reward = actionTerminationEvent.reward };
                // compositeEpisodeStatistics.actionStatistics[actionTerminationEvent.action].steps.Add(actionTerminationEvent.localStep);
                // compositeEpisodeStatistics.actionStatistics[actionTerminationEvent.action].rewards.Add(actionTerminationEvent.reward);
                string action = actionTerminationEvent.action;
                compositeEpisodeStatistics.actionStatistics[action].episodes.Add(episodeStatistics);
                compositeEpisodeStatistics.actionStatistics[action].episodeCount += 1;

                // this action has previously violated an acc
                if (accViolationStepTemp.ContainsKey(action))
                {
                    var acc = accViolationStepTemp[action].Item1;
                    var violationBTStep = accViolationStepTemp[action].Item2;
                    bool successfullyRecovered = true;
                    int stepsToRecover = actionTerminationEvent.btStep - actionTerminationEvent.localStep - violationBTStep;
                    accViolationStepTemp.Remove(action);

                    trackACCRecovery(compositeEpisodeStatistics, action, acc, stepsToRecover, successfullyRecovered);
                }

                if (_event is PostConditionReachedEvent)
                {
                    compositeEpisodeStatistics.postConditionReachedCount++;
                    compositeEpisodeStatistics.actionStatistics[action].postConditionReachedCount++;
                    episodeStatistics.cause = ActionTerminationCause.PostConditionReached;
                }

                else if (_event is ACCViolatedEvent)
                {
                    compositeEpisodeStatistics.accViolatedCount++;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedCount++;
                    episodeStatistics.cause = ActionTerminationCause.ACCViolated;

                    var accViolatedEvent = _event as ACCViolatedEvent;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[accViolatedEvent.acc].count++;
                    // prepare evaluating recovery
                    accViolationStepTemp.Add(action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.btStep));
                }

                else if (_event is LocalResetEvent)
                {
                    compositeEpisodeStatistics.localResetCount++;
                    compositeEpisodeStatistics.actionStatistics[action].localResetCount++;
                    episodeStatistics.cause = ActionTerminationCause.LocalReset;
                }
            }

            else if (_event is GlobalTerminationEvent)
            {
                var globalTerminationEvent = _event as GlobalTerminationEvent;
                compositeEpisodeStatistics.globalSuccess = globalTerminationEvent is GlobalSuccessEvent;
                compositeEpisodeStatistics.steps = globalTerminationEvent.btStep;

                foreach (var action in accViolationStepTemp.Keys)
                {
                    var acc = accViolationStepTemp[action].Item1;
                    int violationBTStep = accViolationStepTemp[action].Item2;
                    var stepsToRecover = globalTerminationEvent.btStep - violationBTStep;
                    bool successfullyRecovered = false;
                    accViolationStepTemp.Remove(action);

                    trackACCRecovery(compositeEpisodeStatistics, action, acc, stepsToRecover, successfullyRecovered);
                }
            }
        }

        return compositeEpisodeStatistics;
    }

    private static void trackACCRecovery(CompositeEpisodeStatistic compositeEpisodeStatistics, string action, string acc, int stepsToRecover, bool successfullyRecovered)
    {
        ACCViolatedStatistic aCCViolatedStatistics = compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[acc];
        aCCViolatedStatistics.recovered.Add(successfullyRecovered);
        aCCViolatedStatistics.stepsToRecover.Add(stepsToRecover);

        List<EpisodeStatistic> episodes = compositeEpisodeStatistics.actionStatistics[action].episodes;
        var previousEpisodeStatistics = episodes[episodes.Count - 2];
        previousEpisodeStatistics.accInfo = new ACCViolatedInfo { accName = acc, stepsToRecover = stepsToRecover, recovered = successfullyRecovered };
    }
}
