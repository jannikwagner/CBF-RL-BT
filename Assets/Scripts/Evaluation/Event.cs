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
    public int compositeEpisodeNumber;
    public int btStep;
}

public enum EventType
{
    PostConditionReached,
    ACCViolated,
    LocalReset,
    ActionGlobalReset,
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

public class ActionStartEvent : ActionEvent { }

public class PostConditionReachedEvent : ActionTerminationEvent { public string postCondition; public EventType type = EventType.PostConditionReached; }
public class ACCViolatedEvent : ActionTerminationEvent { public string acc; public EventType type = EventType.ACCViolated; }
public class LocalResetEvent : ActionTerminationEvent { public EventType type = EventType.LocalReset; }
public class ActionGlobalResetEvent : ActionTerminationEvent { public EventType type = EventType.ActionGlobalReset; }
public class GlobalResetEvent : GlobalTerminationEvent { public EventType type = EventType.GlobalReset; }
public class GlobalSuccessEvent : GlobalTerminationEvent { public EventType type = EventType.GlobalSuccess; }
// public class PreConditionViolatedEvent : Event { public string preCondition; }
