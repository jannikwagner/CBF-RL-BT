using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Assertions;

/**
  * If an agent is in a `SimpleMultiAgentGroup` and its `gameObject` is deactivated, it will be removed from the group.
*/

public interface IAgentProvider
{
    BaseAgent Agent { get; }
}

public interface IAgentSwitcher : IAgentProvider
{
    void Act(BaseAgent agent);
    void AddAgent(BaseAgent agent);
    void AddAgents(BaseAgent[] agents);
    void Reset();
}

public enum AgentSwitcherStatus
{
    Running,
    LocalReset,
    GlobalReset,
}

public class AgentSwitcher : IAgentSwitcher
{
    public List<BaseAgent> agents;
    protected BaseAgent currentAgent;
    protected AgentSwitcherStatus status;
    private AgentSwitchingAsserter asserter;

    public BaseAgent Agent => currentAgent;

    public AgentSwitcher()
    {
        agents = new List<BaseAgent>();
        asserter = new AgentSwitchingAsserter();
    }

    public void AddAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            agent.swtichingAsserter = asserter;
            agents.Add(agent);
            agent.gameObject.SetActive(false);
        }
    }

    public void Act(BaseAgent agent)
    {
        // Debug.Log("AgentSwitcher.Act");
        if (currentAgent != agent)
        {
            SwitchAgent(agent);
        }

        currentAgent.Act();
        status = AgentSwitcherStatus.Running;

        if (currentAgent.EpisodeShouldEnd())
        {
            Debug.Log("LocalReset");
            // currentAgent.EpisodeInterrupted();  // not sure if this should be done TODO // should not be done
            DeactivateAgent();
            currentAgent.ResetEnvLocal();
            currentAgent = null;
            status = AgentSwitcherStatus.LocalReset;
        }
    }

    private void DeactivateAgent()
    {
        Debug.Log("DeactivateAgent: " + currentAgent + ", reward: " + currentAgent.GetCumulativeReward());
        asserter.log(currentAgent, AgentSwitchingAsserter.AgentEvents.Deactivation);
        currentAgent.gameObject.SetActive(false);
    }
    private void ActivateAgent(BaseAgent agent)
    {
        Debug.Log("ActivateAgent " + agent);
        asserter.log(agent, AgentSwitchingAsserter.AgentEvents.Activation);
        currentAgent = agent;
        currentAgent.gameObject.SetActive(true);
    }

    protected void SwitchAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            throw new Exception("Agent not registered");
        }
        if (currentAgent == agent)
        {
            throw new Exception("Agent already active");
        }

        if (currentAgent != null && status == AgentSwitcherStatus.Running)
        {
            DeactivateAgent();
        }

        ActivateAgent(agent);
    }

    public void Reset()
    {
        // currentAgent.SetReward(0f); // don't!
        DeactivateAgent();
        foreach (var item in agents)
        {
            item.gameObject.SetActive(false);
        }
        currentAgent = null;
    }

    public void AddAgents(BaseAgent[] agents)
    {
        foreach (var agent in agents)
        {
            this.AddAgent(agent);
        }
    }
}

public class AgentSwitchingAsserter
{
    public enum AgentEvents
    {
        Activation,
        EpisodeBegin,
        OnActionReceived,
        Deactivation,
    }
    private enum AgentStates
    {
        Inactive,
        Active,
        EpisodeBegun,
        ActionReceived,
    }
    public BaseAgent currentAgent;
    private AgentStates state;
    public void log(BaseAgent agent, AgentEvents agentEvent)
    {
        Assert.IsTrue(agent != null);
        switch (agentEvent)
        {
            case AgentEvents.Activation:
                Assert.IsTrue(state == AgentStates.Inactive);
                Assert.IsTrue(currentAgent == null);
                state = AgentStates.Active;
                currentAgent = agent;
                break;
            case AgentEvents.EpisodeBegin:
                Assert.IsTrue(state == AgentStates.Active);
                Assert.IsTrue(currentAgent == agent);
                state = AgentStates.EpisodeBegun;
                break;
            case AgentEvents.OnActionReceived:
                Assert.IsTrue(state == AgentStates.EpisodeBegun || state == AgentStates.ActionReceived);
                Assert.IsTrue(currentAgent == agent);
                state = AgentStates.ActionReceived;
                break;
            case AgentEvents.Deactivation:
                Assert.IsTrue(state == AgentStates.ActionReceived);
                Assert.IsTrue(currentAgent == agent);
                state = AgentStates.Inactive;
                currentAgent = null;
                break;
            default:
                throw new Exception("Unknown AgentEvent");
                break;
        }
    }
}

public class AgentSwitcherWithAgentGroup : IAgentSwitcher
{
    /** 
    This is an experiment and probably does not work.
    */
    protected List<BaseAgent> agents;
    protected SimpleMultiAgentGroup m_AgentGroup;
    protected BaseAgent currentAgent;
    protected Dictionary<BaseAgent, AgentSwitcherStatus> statusMap;

    public BaseAgent Agent => currentAgent;


    public AgentSwitcherWithAgentGroup()
    {
        agents = new List<BaseAgent>();
        m_AgentGroup = new SimpleMultiAgentGroup();
        statusMap = new Dictionary<BaseAgent, AgentSwitcherStatus>();
    }

    public void AddAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            agents.Add(agent);
            agent.gameObject.SetActive(false);
            // m_AgentGroup.RegisterAgent(agent);
            statusMap[agent] = AgentSwitcherStatus.GlobalReset;
        }
        // Debug.Log(agents.Count);
        // Debug.Log(m_AgentGroup.GetRegisteredAgents().Count);
    }
    public void AddAgents(BaseAgent[] agents)
    {
        foreach (var agent in agents)
        {
            this.AddAgent(agent);
        }
    }

    public void Act(BaseAgent agent)
    {
        // Debug.Log("AgentSwitcher.Act");
        if (currentAgent != agent)
        {
            SwitchAgent(agent);
        }

        currentAgent.Act();
        statusMap[currentAgent] = AgentSwitcherStatus.Running;

        if (currentAgent.EpisodeShouldEnd())
        {
            Debug.Log("LocalReset");
            currentAgent.ResetEnvLocal();
            statusMap[currentAgent] = AgentSwitcherStatus.LocalReset;
        }
    }

    protected void SwitchAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            // AddAgent(agent);
            throw new Exception("Agent not registered");
        }
        if (currentAgent == agent)
        {
            throw new Exception("Agent already active");
        }

        currentAgent = agent;

        if (statusMap[currentAgent] == AgentSwitcherStatus.Running)
        {
            currentAgent.EndEpisode();
        }
        if (statusMap[currentAgent] == AgentSwitcherStatus.LocalReset)
        {
            currentAgent.EpisodeInterrupted();
        }

        currentAgent.gameObject.SetActive(true);
        m_AgentGroup.RegisterAgent(currentAgent);

        Debug.Log("Switched to " + currentAgent);
        // Debug.Log(agents.Count);
        Debug.Log(m_AgentGroup.GetRegisteredAgents().Count);
    }

    public void Reset()
    {
        m_AgentGroup.GroupEpisodeInterrupted();
        foreach (var agent in agents)
        {
            agent.gameObject.SetActive(false);
            statusMap[agent] = AgentSwitcherStatus.GlobalReset;
        }
    }
}
