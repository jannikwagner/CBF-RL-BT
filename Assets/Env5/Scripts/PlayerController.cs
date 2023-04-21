using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Env5
{
    public class PlayerController : MonoBehaviour
    {
        public Transform player;
        public Rigidbody rb;
        public EnvController env;
        public float closenessDistance = 3.0f;
        public float forceFactor = 10f;
        // Start is called before the first frame update
        public float maxSpeed = 10f;

        public void ApplyForce(Vector3 force)
        {
            rb.AddForce(force * rb.mass * forceFactor);

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        public float DistanceToTarget()
        {
            return Vector3.Distance(player.position, env.buttonTrigger.position);
        }
        public float DistanceToGoalTrigger()
        {
            return Vector3.Distance(player.position, env.goalTrigger.position);
        }

        internal bool IsCloseToTarget()
        {
            var condition = DistanceToTarget() < closenessDistance;
            return condition;
        }
        internal bool IsCloseToGoalTrigger()
        {
            var condition = DistanceToGoalTrigger() < closenessDistance;
            return condition;
        }
    }
}
