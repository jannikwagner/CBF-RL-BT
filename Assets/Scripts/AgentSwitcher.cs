using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AgentSwitcher : MonoBehaviour
{
    public List<BaseAgent> agents;
    protected SimpleMultiAgentGroup m_AgentGroup;
    private int currentAgent = 0;

    void Start()
    {
        m_AgentGroup = new SimpleMultiAgentGroup();

        for (int i = 0; i < agents.Count; i++)
        {
            m_AgentGroup.RegisterAgent(agents[i]);
        }
    }

    void Act(BaseAgent agent)
    {
    }
}
