using System;
using System.Collections;
using System.Collections.Generic;
using Env5;
using Unity.MLAgents;
using UnityEngine;

/**
  * If an agent is in a `SimpleMultiAgentGroup` and its `gameObject` is deactivated, it will be removed from the group.
*/

public interface IAgentSwitcher
{
    void Act(BaseAgent agent);
    void AddAgent(BaseAgent agent);
    void AddAgents(EnvBaseAgent[] agents);
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

    public AgentSwitcher()
    {
        agents = new List<BaseAgent>();
    }

    public void AddAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            agents.Add(agent);
            agent.gameObject.SetActive(false);
        }
        // Debug.Log(agents);
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
            // currentAgent.EpisodeInterrupted();  // not sure if this should be done TODO
            currentAgent.gameObject.SetActive(false);
            currentAgent.ResetEnvLocal();
            status = AgentSwitcherStatus.LocalReset;
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

        if (currentAgent != null && status == AgentSwitcherStatus.Running)
        {
            currentAgent.gameObject.SetActive(false);
        }

        currentAgent = agent;
        Debug.Log("Switched to " + currentAgent);
        currentAgent.gameObject.SetActive(true);

        // foreach (var item in agents)
        // {
        //     if (item != currentAgent)
        //     {
        //         item.gameObject.SetActive(false);
        //     }
        // }
        // Debug.Log(agents.Count);
    }

    public void Reset()
    {
        foreach (var item in agents)
        {
            item.gameObject.SetActive(false);
        }
        currentAgent.ResetEnvGlobal();
        currentAgent = null;
    }

    public void AddAgents(EnvBaseAgent[] agents)
    {
        foreach (var agent in agents)
        {
            this.AddAgent(agent);
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
    protected AgentSwitcherStatus status;

    public AgentSwitcherWithAgentGroup()
    {
        agents = new List<BaseAgent>();
        m_AgentGroup = new SimpleMultiAgentGroup();
    }

    public void AddAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            agents.Add(agent);
            m_AgentGroup.RegisterAgent(agent);
        }
        // Debug.Log(agents.Count);
        // Debug.Log(m_AgentGroup.GetRegisteredAgents().Count);
    }
    public void AddAgents(EnvBaseAgent[] agents)
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
        status = AgentSwitcherStatus.Running;

        if (currentAgent.EpisodeShouldEnd())
        {
            Debug.Log("LocalReset");
            currentAgent.EpisodeInterrupted();  // not sure if this should be done TODO
            currentAgent.ResetEnvLocal();
            status = AgentSwitcherStatus.LocalReset;
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

        if (currentAgent != null && status == AgentSwitcherStatus.Running)
        {
            currentAgent.EndEpisode();
        }

        currentAgent = agent;
        currentAgent.EpisodeInterrupted();  // So that a new episode is started

        Debug.Log("Switched to " + currentAgent);
        // Debug.Log(agents.Count);
        // Debug.Log(m_AgentGroup.GetRegisteredAgents().Count);
    }

    public void Reset()
    {
        m_AgentGroup.GroupEpisodeInterrupted();
        status = AgentSwitcherStatus.GlobalReset;
    }
}
