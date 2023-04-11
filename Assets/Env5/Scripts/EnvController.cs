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
        bridgeDown.SetActive(false);
        bridgeUp.SetActive(true);
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

    bool Button1Pressed()
    {
        return Vector3.Distance(target1Transform.position, button1Transform.position) < 1.0f
        || Vector3.Distance(target2Transform.position, button1Transform.position) < 1.0f;
    }
    bool Button2Pressed()
    {
        return Vector3.Distance(target1Transform.position, button2Transform.position) < 1.0f
        || Vector3.Distance(target2Transform.position, button2Transform.position) < 1.0f;
    }
    bool buttonsPressed()
    {
        return Button1Pressed();  // && Button2Pressed();
    }

    bool win()
    {
        return Vector3.Distance(poleTransform.position, goalTransform.position) < 1.0f;
    }





}
