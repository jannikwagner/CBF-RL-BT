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
        private float closenessDistance = 3.0f;
        private float forceFactor = 10f;
        // Start is called before the first frame update
        internal float maxSpeed = 10f;

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
            return Vector3.Distance(player.position, env.target.position);
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
        void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (collision.gameObject.tag == "Target")
            {
                if (!env.ButtonPressed())
                {
                    StartControlTarget();
                }
            }
            else if (collision.gameObject.tag == "GoalTrigger")
            {
                if (env.ButtonPressed() && !env.GoalPressed())
                {
                    StartControlGoalTrigger();
                }
            }
            else if (collision.gameObject.tag == "Button")
            {
                StopControlTarget(true);
            }
            else if (collision.gameObject.tag == "Goal")
            {
                StopControlGoalTrigger(true);
            }
        }
        // void OnCollisionStay(UnityEngine.Collision collision)
        // {
        //     OnCollisionEnter(collision);
        // }

        public void StopControl()
        {
            StopControlTarget();
            StopControlGoalTrigger();
        }

        private void StopControlTarget(bool press = false)
        {
            if (IsControllingTarget())
            {
                ControlOther controlOther = this.GetComponent<ControlOther>();
                controlOther.enabled = false;
                env.target.position = new Vector3(env.button.position.x, env.button.position.y + 0.5f, env.button.position.z);
                env.target.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToTarget for one FixedUpdate.
                if (press)
                    env.button.GetComponentInParent<CollisionDetector>().Pressed = true;
            }
        }

        public bool IsControllingTarget()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            return controlOther.enabled && controlOther.other == env.target.GetComponent<Rigidbody>();
        }

        private void StopControlGoalTrigger(bool press = false)
        {
            if (IsControllingGoalTrigger())
            {
                ControlOther controlOther = this.GetComponent<ControlOther>();
                controlOther.enabled = false;
                env.goalTrigger.position = new Vector3(env.goal.position.x, env.goal.position.y + 0.5f, env.goal.position.z);
                env.goalTrigger.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToGoalTrigger for one FixedUpdate.
                if (press)
                    env.goal.GetComponentInParent<CollisionDetector>().Pressed = true;
            }
        }

        public bool IsControllingGoalTrigger()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            return controlOther.enabled && controlOther.other == env.goalTrigger.GetComponent<Rigidbody>();
        }

        private void StartControlGoalTrigger()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            controlOther.enabled = true;
            controlOther.other = env.goalTrigger.GetComponent<Rigidbody>();
        }

        private void StartControlTarget()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            controlOther.enabled = true;
            controlOther.other = env.target.GetComponent<Rigidbody>();
        }
    }
}
