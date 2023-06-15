
using System;
using System.Collections.Generic;
using BTTest;

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
        compositeEpisodeStatistics.compositeEpisodeNumber = compositeEpisodeEvents[0].compositeEpisode;
        var accViolationStepTemp = new Dictionary<string, Tuple<string, int>>();

        foreach (Event _event in compositeEpisodeEvents)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;
                var episodeStatistics = new EpisodeStatistic { steps = actionTerminationEvent.localStep, reward = actionTerminationEvent.reward };
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
                    episodeStatistics.terminationCause = ActionTerminationCause.PostConditionReached;
                }

                else if (_event is ACCViolatedEvent)
                {
                    compositeEpisodeStatistics.accViolatedCount++;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedCount++;
                    episodeStatistics.terminationCause = ActionTerminationCause.ACCViolated;

                    var accViolatedEvent = _event as ACCViolatedEvent;
                    compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[accViolatedEvent.acc].count++;
                    // prepare evaluating recovery
                    accViolationStepTemp.Add(action, Tuple.Create(accViolatedEvent.acc, accViolatedEvent.btStep));
                }

                else if (_event is LocalResetEvent)
                {
                    compositeEpisodeStatistics.localResetCount++;
                    compositeEpisodeStatistics.actionStatistics[action].localResetCount++;
                    episodeStatistics.terminationCause = ActionTerminationCause.LocalReset;
                }

                else if (_event is ActionGlobalResetEvent)
                {
                    episodeStatistics.terminationCause = ActionTerminationCause.GlobalReset;
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
        previousEpisodeStatistics.accInfo = new ACCViolatedInfo { name = acc, stepsToRecover = stepsToRecover, recovered = successfullyRecovered };
    }
}
