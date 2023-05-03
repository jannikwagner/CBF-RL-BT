using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOther : MonoBehaviour
{
    public Rigidbody other;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        // Debug.Log(other.velocity);
        // Debug.Log(GetComponent<Rigidbody>().velocity);
        other.velocity = this.GetComponent<Rigidbody>().velocity;
        var position = this.GetComponent<Transform>().position;
        var controlledPosition = new Vector3(position.x, position.y + 1, position.z);
        other.GetComponent<Transform>().position = controlledPosition;
    }
}
