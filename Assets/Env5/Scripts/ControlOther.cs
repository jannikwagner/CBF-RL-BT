using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOther : MonoBehaviour
{
    public Rigidbody other;
    void FixedUpdate()
    {
        other.velocity = this.GetComponent<Rigidbody>().velocity;
        var position = this.GetComponent<Transform>().position;
        var controlledPosition = new Vector3(position.x, position.y + 1.1f, position.z);
        other.GetComponent<Transform>().position = controlledPosition;
    }
}
