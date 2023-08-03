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

                Vector2 safePoint = FindSafePoint(player.position, rb.velocity, env.button1.position);
                env.trigger1.position = new Vector3(safePoint.x, env.button1.position.y + 0.5f, safePoint.y);
                env.trigger1.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToT1 for one FixedUpdate.
                if (press)
                {
                    env.button1.GetComponentInParent<CollisionDetector>().ManuallyAdd(env.trigger1.gameObject.tag);
                }
            }
        }

        private static Vector2 FindSafePoint(Vector3 c1_3d, Vector3 r1_3d, Vector3 c2_3d)
        {
            var c1 = new Vector2(c1_3d.x, c1_3d.z);
            var r1 = new Vector2(r1_3d.x, r1_3d.z).normalized;
            var c2 = new Vector2(c2_3d.x, c2_3d.z);

            var t1 = Vector2.Dot(c2 - c1, r1);
            var p = c1 + t1 * r1;
            var d2 = p - c2;
            var r2 = d2.normalized;
            var x = p - r2 * 1.5f;
            return x;
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

                var safePoint = FindSafePoint(player.position, rb.velocity, env.button2.position);
                env.trigger2.position = new Vector3(safePoint.x, env.button2.position.y + 0.5f, safePoint.y);
                env.trigger2.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                // The physics engine would be one FixedUpdate behind if we don't do this. This would lead to the BT executing MoveToGoalTrigger for one FixedUpdate.
                if (press)
                {
                    env.button2.GetComponentInParent<CollisionDetector>().ManuallyAdd(env.trigger2.gameObject.tag);
                }
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
