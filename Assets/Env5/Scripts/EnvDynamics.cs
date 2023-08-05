using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PlayerPosVelDynamics : IDynamicsProvider
    {
        EnvBaseAgent agent;
        public PlayerPosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var velocity = agent.controller.rb.velocity;
            var acceleration = agent.GetAcceleration(action);
            // apply acceleration before velocity! needed to be safe because of discretization of continuous system
            // note: applying all steps accels at once is safer than actually necessary, but also simpler.
            // 0.5 is apparently also safe. 0.5 corresponds to proper integration!
            // TODO: move deltaTime based logic for integration into a new dedicated method IDynamicsProvider.deltaX() (#41)
            float deltaTime = Time.fixedDeltaTime * agent.ActionsPerDecision;
            velocity = velocity + 0.5f * acceleration * deltaTime;
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition;
            var velocity = agent.controller.rb.velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
    public class PlayerTrigger1PosVelDynamics : IDynamicsProvider
    {
        // relative position and velocity of player to trigger1
        EnvBaseAgent agent;
        public PlayerTrigger1PosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var trigger1Velocity = agent.controller.env.trigger1.GetComponentInParent<Rigidbody>().velocity;
            var velocity = agent.controller.rb.velocity - trigger1Velocity;
            var acceleration = agent.GetAcceleration(action); // + 0.5f * trigger1Velocity; // not sure why I added this here
            // TODO: move deltaTime based logic for integration into a new dedicated method IDynamicsProvider.deltaX() (#41)
            float deltaTime = Time.fixedDeltaTime * agent.ActionsPerDecision;
            velocity = velocity + 0.5f * acceleration * deltaTime;
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition - agent.controller.env.trigger1.localPosition;
            var velocity = agent.controller.rb.velocity - agent.controller.env.trigger1.GetComponentInParent<Rigidbody>().velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
}