using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Env5
{
    public class DebugController : MonoBehaviour
    {
        public PlayerController controller;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            controller.ApplyAcc(new Vector3(x, 0, z));
        }
    }
}
