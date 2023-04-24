using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AgentSwitcher : MonoBehaviour
{
    public List<BaseAgent> agents;
    protected SimpleMultiAgentGroup m_AgentGroup;
    protected BaseAgent currentAgent;

    void Start()
    {
        m_AgentGroup = new SimpleMultiAgentGroup();

        for (int i = 0; i < agents.Count; i++)
        {
            m_AgentGroup.RegisterAgent(agents[i]);
        }
    }

    public void AddAgent(BaseAgent agent)
    {
        if (agents == null)
        {
            agents = new List<BaseAgent>();
        }
        if (!agents.Contains(agent))
        {
            agents.Add(agent);
            m_AgentGroup.RegisterAgent(agent);
        }
        Debug.Log(agents);
        Debug.Log(m_AgentGroup.GetRegisteredAgents());
    }

    public void Act(BaseAgent agent)
    {
        Debug.Log("AgentSwitcher.Act");
        if (currentAgent != agent)
        {
            SwitchAgent(agent);
        }
        currentAgent.Act();

        if (currentAgent.EpisodeShouldEnd())
        {
            currentAgent.EpisodeInterrupted();  // not sure if this should be done TODO
            currentAgent.gameObject.SetActive(false);
            currentAgent.ResetEnv();
        }
    }

    protected void SwitchAgent(BaseAgent agent)
    {
        if (!agents.Contains(agent))
        {
            // AddAgent(agent);
            throw new Exception("Agent not registered");
        }

        if (currentAgent != null)
        {
            currentAgent.gameObject.SetActive(false);
            // currentAgent.enabled = false;
        }
        currentAgent = agent;
        currentAgent.gameObject.SetActive(true);
        // TODO: possibly RegisterAgent each time?
        // currentAgent.enabled = true;

        Debug.Log(currentAgent);
        Debug.Log(agents);
        Debug.Log(m_AgentGroup.GetRegisteredAgents());
    }
}
