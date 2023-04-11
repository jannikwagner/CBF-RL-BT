using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    public Transform playerTransform;
    public Transform button1Transform;
    public Transform button2Transform;
    public Transform target1Transform;
    public Transform target2Transform;
    public Transform poleTransform;
    public GameObject bridgeDown;
    public GameObject bridgeUp;
    public Transform goalTransform;

    void Start()
    {
        Initialize();
    }

    void FixedUpdate()
    {
        if (buttonsPressed())
        {
            bridgeDown.SetActive(true);
            bridgeUp.SetActive(false);
        }
        else
        {
            bridgeDown.SetActive(false);
            bridgeUp.SetActive(true);
        }

        if (win())
        {
            Debug.Log("You win!");
        }
    }

    public bool Button1Pressed()
    {
        return Vector3.Distance(target1Transform.position, button1Transform.position) < 1.0f
        || Vector3.Distance(target2Transform.position, button1Transform.position) < 1.0f;
    }
    public bool Button2Pressed()
    {
        return Vector3.Distance(target1Transform.position, button2Transform.position) < 1.0f
        || Vector3.Distance(target2Transform.position, button2Transform.position) < 1.0f;
    }
    public bool buttonsPressed()
    {
        return Button1Pressed();  // && Button2Pressed();
    }

    public bool win()
    {
        return Vector3.Distance(poleTransform.position, goalTransform.position) < 1.0f;
    }

    public void Initialize()
    {
        float minX = -25;
        float maxX = 10;
        float minZ = -23;
        float maxZ = 14;
        float groundZ = 0.5f;

        playerTransform.position = new Vector3(Random.Range(minX, maxX), groundZ, Random.Range(minZ, maxZ));
        // button1Transform.position = new Vector3(0, 0.5f, 0);
        // button2Transform.position = new Vector3(0, 0.5f, 0);
        // target1Transform.position = new Vector3(0, 0.5f, 0);
        // target2Transform.position = new Vector3(0, 0.5f, 0);
        // poleTransform.position = new Vector3(0, 0.5f, 0);
        // goalTransform.position = new Vector3(0, 0.5f, 0);
        bridgeDown.SetActive(false);
        bridgeUp.SetActive(true);
    }





}
