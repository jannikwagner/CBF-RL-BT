using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Env5
{
    public class CollisionDetector : MonoBehaviour
    {
        // Start is called before the first frame update
        public string targetTag;
        private bool pressed = false;

        public bool Pressed { get => pressed; }

        void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (collision.gameObject.tag == targetTag)
            {
                pressed = true;
            }
        }
        void OnCollisionExit(UnityEngine.Collision collision)
        {
            if (collision.gameObject.tag == targetTag)
            {
                pressed = false;
            }
        }
    }
}
