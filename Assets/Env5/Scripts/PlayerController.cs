using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerTransform;
    public Rigidbody rb;
    public Env5Controller env5Controller;
    public Transform poleTransform;
    // Start is called before the first frame update
    private bool holdingPole = false;
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var fx = Input.GetAxis("Horizontal");
        var fz = Input.GetAxis("Vertical");
        var movement = new Vector3(fx, 0, fz);
        rb.AddForce(movement * rb.mass * 10f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(new Vector3(0, 1, 0) * rb.mass * 1f, ForceMode.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (DistanceToPole() < 1.0f)
            {
                holdingPole = !holdingPole;
            }
        }

        ControlPole();
    }

    private float DistanceToPole()
    {
        return Vector3.Distance(playerTransform.position, poleTransform.position);
    }

    void ControlPole()
    {
        if (holdingPole)
        {
            poleTransform.position = playerTransform.position + new Vector3(0.6f, 0f, 0f);
        }
    }
}
