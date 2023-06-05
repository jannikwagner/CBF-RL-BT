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
        var runStatistics = new RunStatistics(actions);
        var accViolationStepTemp = new Dictionary<string, Tuple<string, int>>();

        foreach (Event _event in runEvents)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                runStatistics.actionStatistics[actionTerminationEvent.action].steps.Add(actionTerminationEvent.localStep);
                runStatistics.actionStatistics[actionTerminationEvent.action].rewards.Add(actionTerminationEvent.reward);
                runStatistics.actionStatistics[actionTerminationEvent.action].episodes += 1;

                // this action has previously violated an acc
                if (accViolationStepTemp.ContainsKey((actionTerminationEvent.action)))
                {
                    var acc = accViolationStepTemp[actionTerminationEvent.action].Item1;
                    var violationGlobalStep = accViolationStepTemp[actionTerminationEvent.action].Item2;
                    accViolationStepTemp.Remove(actionTerminationEvent.action);
                    runStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[acc].recovered.Add(true);
                    runStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[acc].stepsToRecover.Add(actionTerminationEvent.globalStep - actionTerminationEvent.localStep - violationGlobalStep);
                }

                if (_event is PostConditionReachedEvent)
                {
                    runStatistics.postConditionReachedCount++;
                    runStatistics.actionStatistics[actionTerminationEvent.action].postConditionReachedCount++;
                }

                else if (_event is ACCViolatedEvent)
                {
                    runStatistics.accViolatedCount++;
                    runStatistics.actionStatistics[actionTerminationEvent.action].accViolatedCount++;

                    var accViolatedEvent = _event as ACCViolatedEvent;
                    runStatistics.actionStatistics[actionTerminationEvent.action].accViolatedStatistics[accViolatedEvent.acc].count++;
                    // prepare evaluating recovery
                    accViolationStepTemp.Add(actionTerminationEvent.action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.globalStep));
                }

                else if (_event is LocalResetEvent)
                {
                    runStatistics.localResetCount++;
                    runStatistics.actionStatistics[actionTerminationEvent.action].localResetCount++;
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
                    var step = accViolationStepTemp[action].Item2;
                    runStatistics.actionStatistics[action].accViolatedStatistics[acc].recovered.Add(false);
                    runStatistics.actionStatistics[action].accViolatedStatistics[acc].stepsToRecover.Add(globalTerminationEvent.globalStep - step);
                    accViolationStepTemp.Remove(action);
                }
            }
        }

        return runStatistics;
    }
}
