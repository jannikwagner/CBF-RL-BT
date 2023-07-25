using System;
using System.Collections.Generic;
using BTTest;
using UnityEngine;

public class CompositeEpisodeEvaluator
{
    private IEnumerable<LearningActionAgentSwitcher> actions;

    public CompositeEpisodeEvaluator(IEnumerable<LearningActionAgentSwitcher> actions)
    {
        this.actions = actions;
    }

    public CompositeEpisodeStatistic EvaluateCompositeEpisode(List<Event> events)
    {
        string currentAction = null;

        // first representation
        var compositeEpisodeStatistics = new CompositeEpisodeStatistic(actions);
        compositeEpisodeStatistics.compositeEpisodeNumber = events[0].compositeEpisodeNumber;

        // alternative representation
        var episodes = new List<EpisodeRecord>();

        var accViolationStepTemp = new Dictionary<string, Tuple<string, int, EpisodeStatistic, EpisodeRecord>>();
        int localEpisodeNumber = 0;

        foreach (Event _event in events)
        {
            if (_event is ActionStartEvent)
            {
                if (currentAction != null)
                {
                    // TODO: fix this bug instead of ignoring cases where it happens
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(events));
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(_event));
                    // throw new Exception("ActionStartEvent received while another action is still active");
                    Debug.Log("Inconsistency: ActionStartEvent received while another action is still active");
                    return null;

                }
                var actionStartEvent = _event as ActionStartEvent;
                currentAction = actionStartEvent.action;
            }

            else if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                if (currentAction == null)
                {
                    // TODO: fix this bug instead of ignoring cases where it happens
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(events));
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(_event));
                    // throw new Exception("ActionTerminationEvent received while no action is active");
                    Debug.Log("Inconsistency: ActionTerminationEvent received while no action is active");
                    return null;
                }
                if (currentAction != actionTerminationEvent.action)
                {
                    // not observed so far
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(events));
                    Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(_event));
                    // throw new Exception("ActionTerminationEvent received for action other than the currently active one");
                    Debug.Log("Inconsistency: ActionTerminationEvent received for action other than the currently active one");
                    return null;
                }
                currentAction = null;

                var episodeStatistics = new EpisodeStatistic { localSteps = actionTerminationEvent.localStep, reward = actionTerminationEvent.reward, localEpisodeNumber = localEpisodeNumber };
                string action = actionTerminationEvent.action;
                compositeEpisodeStatistics.actionStatistics[action].episodes.Add(episodeStatistics);
                compositeEpisodeStatistics.actionStatistics[action].episodeCount += 1;

                var episode = new EpisodeRecord
                {
                    compositeEpisodeNumber = events[0].compositeEpisodeNumber,
                    localEpisodeNumber = localEpisodeNumber++,
                    actionName = action,
                    localSteps = actionTerminationEvent.localStep,
                    reward = actionTerminationEvent.reward
                };
                episodes.Add(episode);

                // this action has previously violated an acc
                if (accViolationStepTemp.ContainsKey(action))
                {
                    var acc = accViolationStepTemp[action].Item1;
                    var violationBTStep = accViolationStepTemp[action].Item2;
                    var episodeStatisticWithViolation = accViolationStepTemp[action].Item3;
                    var episodeWithViolation = accViolationStepTemp[action].Item4;

                    bool successfullyRecovered = true;
                    int stepsToRecover = actionTerminationEvent.btStep - actionTerminationEvent.localStep - violationBTStep;
                    accViolationStepTemp.Remove(action);

                    trackACCRecovery(compositeEpisodeStatistics, action, acc, stepsToRecover, successfullyRecovered, episodeStatisticWithViolation, episodeWithViolation);
                }

                if (_event is PostConditionReachedEvent)
                {
                    compositeEpisodeStatistics.postConditionReachedCount++;
                    compositeEpisodeStatistics.actionStatistics[action].postConditionReachedCount++;
                    episodeStatistics.terminationCause = ActionTerminationCause.PostConditionReached;

                    episode.terminationCause = ActionTerminationCause.PostConditionReached;
                }

                else if (_event is ACCViolatedEvent)
                {
                    var accViolatedEvent = _event as ACCViolatedEvent;
                    // prepare evaluating recovery
                    accViolationStepTemp.Add(action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.btStep, episodeStatistics, episode));

                    compositeEpisodeStatistics.accViolatedCount++;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedCount++;
                    episodeStatistics.terminationCause = ActionTerminationCause.ACCViolated;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[accViolatedEvent.acc].count++;

                    episode.terminationCause = ActionTerminationCause.ACCViolated;
                    episode.accName = accViolatedEvent.acc;
                }

                else if (_event is LocalResetEvent)
                {
                    compositeEpisodeStatistics.localResetCount++;
                    compositeEpisodeStatistics.actionStatistics[action].localResetCount++;
                    episodeStatistics.terminationCause = ActionTerminationCause.LocalReset;

                    episode.terminationCause = ActionTerminationCause.LocalReset;
                }

                else if (_event is ActionGlobalResetEvent)
                {
                    episodeStatistics.terminationCause = ActionTerminationCause.GlobalReset;

                    episode.terminationCause = ActionTerminationCause.GlobalReset;
                }
            }

            else if (_event is GlobalTerminationEvent)
            {
                var globalTerminationEvent = _event as GlobalTerminationEvent;

                compositeEpisodeStatistics.globalSuccess = globalTerminationEvent is GlobalSuccessEvent;
                compositeEpisodeStatistics.globalSteps = globalTerminationEvent.btStep;

                foreach (var episode in episodes)
                {
                    episode.globalSuccess = globalTerminationEvent is GlobalSuccessEvent;
                    episode.globalSteps = globalTerminationEvent.btStep;
                }

                foreach (var action in accViolationStepTemp.Keys)
                {
                    var acc = accViolationStepTemp[action].Item1;
                    int violationBTStep = accViolationStepTemp[action].Item2;
                    var episodeStatisticWithViolation = accViolationStepTemp[action].Item3;
                    var episodeWithViolation = accViolationStepTemp[action].Item4;

                    var stepsToRecover = globalTerminationEvent.btStep - violationBTStep;
                    bool successfullyRecovered = false;

                    trackACCRecovery(compositeEpisodeStatistics, action, acc, stepsToRecover, successfullyRecovered, episodeStatisticWithViolation, episodeWithViolation);
                }
                accViolationStepTemp.Clear();
            }
        }

        return compositeEpisodeStatistics;
    }

    private static void trackACCRecovery(CompositeEpisodeStatistic compositeEpisodeStatistics, string action, string acc, int stepsToRecover, bool successfullyRecovered, EpisodeStatistic episodeStatistic, EpisodeRecord episode)
    {
        // representation 1
        ACCViolatedStatistic aCCViolatedStatistics = compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[acc];
        aCCViolatedStatistics.recovered.Add(successfullyRecovered);
        aCCViolatedStatistics.stepsToRecover.Add(stepsToRecover);
        episodeStatistic.accInfo = new ACCViolatedInfo { accName = acc, accStepsToRecover = stepsToRecover, accRecovered = successfullyRecovered };

        // representation 2
        episode.accName = acc;  // not necessary, already set before
        episode.accStepsToRecover = stepsToRecover;
        episode.accRecovered = successfullyRecovered;
    }
}
