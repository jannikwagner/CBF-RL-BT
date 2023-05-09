using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Env5
{
    public class CollisionDetector : MonoBehaviour
    {
        // Start is called before the first frame update
        private HashSet<string> touchedObjects = new HashSet<string>();

        public bool Touching(GameObject target)
        {
            return Touching(target.tag);
        }
        public bool Touching(string tag)
        {
            return touchedObjects.Contains(tag);
        }
        public void ManuallyAdd(string tag)
        {
            touchedObjects.Add(tag);
        }
        public void ManuallyRemove(string tag)
        {
            touchedObjects.Remove(tag);
        }

        void OnCollisionEnter(UnityEngine.Collision collision)
        {
            touchedObjects.Add(collision.gameObject.tag);
        }
        void OnCollisionStay(UnityEngine.Collision collision)
        {
            touchedObjects.Add(collision.gameObject.tag);
        }
        void OnCollisionExit(UnityEngine.Collision collision)
        {
            touchedObjects.Remove(collision.gameObject.tag);
        }
    }
}
