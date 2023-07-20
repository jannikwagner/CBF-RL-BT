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
        private float maxAcc = 10f;
        internal float maxSpeed = 10f;

        public float MaxAcc { get => maxAcc; }

        public void ApplyAcceleration(Vector3 acceleration)
        {
            rb.AddForce(acceleration, ForceMode.Acceleration);

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        public float DistanceToTrigger1()
        {
            return Vector3.Distance(player.position, env.trigger1.position);
        }
        public float DistanceToTrigger2()
        {
            return Vector3.Distance(player.position, env.trigger2.position);
        }

        internal bool IsCloseToTrigger1()
        {
            var condition = DistanceToTrigger1() < closenessDistance;
            return condition;
        }
        internal bool IsCloseToTrigger2()
        {
            var condition = DistanceToTrigger2() < closenessDistance;
            return condition;
        }
        void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (collision.gameObject.tag == "Target")
            {
                if (!env.Button1Pressed())
                {
                    StartControlTrigger1();
                }
            }
            else if (collision.gameObject.tag == "GoalTrigger")
            {
                if (env.Button1Pressed() && !env.button2Pressed())
                {
                    StartControlTrigger2();
                }
            }
            else if (collision.gameObject.tag == "Button")
            {
                StopControlTrigger1(true);
            }
            else if (collision.gameObject.tag == "Goal")
            {
                StopControlTrigger2(true);
            }
        }

        public void StopControl()
        {
            StopControlTrigger1();
            StopControlTrigger2();
        }

        private void StopControlTrigger1(bool press = false)
        {
            if (IsControllingT1())
            {
                ControlOther controlOther = this.GetComponent<ControlOther>();
                controlOther.enabled = false;
                env.trigger1.position = new Vector3(env.button1.position.x, env.button1.position.y + 0.5f, env.button1.position.z);
                env.trigger1.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToT1 for one FixedUpdate.
                if (press)
                    env.button1.GetComponentInParent<CollisionDetector>().ManuallyAdd(env.trigger1.gameObject.tag);
            }
        }

        public bool IsControllingT1()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            return controlOther.enabled && controlOther.other == env.trigger1.GetComponent<Rigidbody>();
        }

        private void StopControlTrigger2(bool press = false)
        {
            if (IsControllingT2())
            {
                ControlOther controlOther = this.GetComponent<ControlOther>();
                controlOther.enabled = false;
                env.trigger2.position = new Vector3(env.button2.position.x, env.button2.position.y + 0.5f, env.button2.position.z);
                env.trigger2.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToGoalTrigger for one FixedUpdate.
                if (press)
                    env.button2.GetComponentInParent<CollisionDetector>().ManuallyAdd(env.trigger2.gameObject.tag);
            }
        }

        public bool IsControllingT2()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            return controlOther.enabled && controlOther.other == env.trigger2.GetComponent<Rigidbody>();
        }

        private void StartControlTrigger2()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            controlOther.enabled = true;
            controlOther.other = env.trigger2.GetComponent<Rigidbody>();
        }

        private void StartControlTrigger1()
        {
            ControlOther controlOther = this.GetComponent<ControlOther>();
            controlOther.enabled = true;
            controlOther.other = env.trigger1.GetComponent<Rigidbody>();
        }
        public bool TouchingBridgeDown()
        {
            var collisionDetector = GetComponent<CollisionDetector>();
            return collisionDetector.Touching(env.bridgeDown);
        }
    }
}
