using System.Collections.Generic;

public class CompositeEpisodeStatistic
{
    public int compositeEpisodeNumber = -1;
    public bool globalSuccess = false;
    public int globalSteps = -1;
    public int postConditionReachedCount = 0;
    public int higherPostConditionReachedCount = 0;
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
    public int higherPostConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ACCViolatedStatistic> accViolatedStatistics = new Dictionary<string, ACCViolatedStatistic>();

    public ActionStatistic(BTTest.LearningActionAgentSwitcher action)
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
    public int localSteps = -1;
    public float reward = 0;
    public int localEpisodeNumber = -1;
    public string postCondition = null;
    public ActionTerminationCause terminationCause;
    public ACCViolatedInfo accInfo = null;
}

public class ACCViolatedInfo
{
    public string accName;
    public int accStepsToRecover;
    public bool accRecovered;
}

public class ACCViolatedStatistic
{
    public string accName;
    public int count = 0;
    public List<int> stepsToRecover = new List<int>();
    public List<bool> recovered = new List<bool>();
}

public enum ActionTerminationCause
{
    PostConditionReached,
    ACCViolated,
    LocalReset,
    GlobalReset,
    HigherPostConditionReached,
}

// different approach, properly collected, but currently not serialized
public class EpisodeRecord
{
    public int compositeEpisodeNumber = -1;
    public int localEpisodeNumber = -1;
    public string actionName = null;
    public int localSteps = 0;
    public float reward = 0;
    public ActionTerminationCause terminationCause;
    public string accName = null;
    public int accStepsToRecover = -1;
    public bool accRecovered = false;
    public bool globalSuccess = false;
    public int globalSteps = -1;
    public string postCondition = null;
}
