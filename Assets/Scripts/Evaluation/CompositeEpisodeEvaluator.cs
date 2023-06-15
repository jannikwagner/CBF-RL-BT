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

    public CompositeEpisodeStatistic EvaluateCompositeEpisode(List<Event> events)
    {
        // first representation
        var compositeEpisodeStatistics = new CompositeEpisodeStatistic(actions);
        compositeEpisodeStatistics.compositeEpisodeNumber = events[0].compositeEpisodeNumber;

        // alternative representation
        var episodes = new List<EpisodeRecord>();

        var accViolationStepTemp = new Dictionary<string, Tuple<string, int, EpisodeStatistic, EpisodeRecord>>();
        int localEpisodeNumber = 0;

        foreach (Event _event in events)
        {
            if (_event is ActionTerminationEvent)
            {
                var actionTerminationEvent = _event as ActionTerminationEvent;

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
        ACCViolatedStatistic aCCViolatedStatistics = compositeEpisodeStatistics.actionStatistics[action].accViolatedStatistics[acc];
        aCCViolatedStatistics.recovered.Add(successfullyRecovered);
        aCCViolatedStatistics.stepsToRecover.Add(stepsToRecover);

        List<EpisodeStatistic> episodes = compositeEpisodeStatistics.actionStatistics[action].episodes;
        // if successfully recovered, the second last (because the episode where it is recovered is last),
        // if not, the last, because we did not have the action again, otherwise it would have recovered
        episodeStatistic.accInfo = new ACCViolatedInfo { accName = acc, accStepsToRecover = stepsToRecover, accRecovered = successfullyRecovered };

        episode.accName = acc;  // not necessary, already set before
        episode.accStepsToRecover = stepsToRecover;
        episode.accRecovered = successfullyRecovered;
    }
}
