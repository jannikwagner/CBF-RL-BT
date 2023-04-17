using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    public Transform player;
    public Transform button;
    public Transform target;
    public Transform pole;
    public Transform goal;
    public GameObject bridgeDown;
    public GameObject bridgeUp;

    void Start()
    {
        Initialize();
    }

    void FixedUpdate()
    {
        if (ButtonPressed())
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

    // public bool Button1Pressed()
    // {
    //     return Vector3.Distance(targetTransform.position, buttonTransform.position) < 1.0f
    //     || Vector3.Distance(target2Transform.position, buttonTransform.position) < 1.0f;
    // }
    // public bool Button2Pressed()
    // {
    //     return Vector3.Distance(targetTransform.position, button2Transform.position) < 1.0f
    //     || Vector3.Distance(target2Transform.position, button2Transform.position) < 1.0f;
    // }

    public bool ButtonPressed()
    {
        return Vector3.Distance(target.position, button.position) < 1.0f;
    }

    public bool win()
    {
        return Vector3.Distance(pole.position, goal.position) < 1.0f;
    }

    public void Initialize()
    {
        float minX = -26;
        float maxX = 10;
        float maxXTarget = 0f;
        float minZ = -23;
        float maxZ = 14;
        float elevatedGroundY = 0.5f;
        float elevatedGroundYTarget = elevatedGroundY - 1;
        float lowerGroundY = elevatedGroundY - 4;
        float lowerGroundYTarget = lowerGroundY - 1;

        float buttonMaxX = -8;
        float buttonTiltStartX = -20;
        float buttonSmallMaxZ = -2;
        float buttonSmallMinZ = -6;

        // player.localPosition = new Vector3(Random.Range(minX, maxX), elevatedGroundY, Random.Range(minZ, maxZ));
        // target.localPosition = new Vector3(Random.Range(minX, maxXTarget), elevatedGroundY, Random.Range(minZ, maxZ));
        // float buttonX = Random.Range(minX, buttonMaxX);
        // float buttonZ = buttonX < buttonTiltStartX ? Random.Range(minZ, maxZ) : Random.Range(buttonSmallMinZ, buttonSmallMaxZ);
        // button.localPosition = new Vector3(buttonX, elevatedGroundYTarget, buttonZ);

        // button2Transform.position = new Vector3(0, 0.5f, 0);
        // target1Transform.position = new Vector3(0, 0.5f, 0);
        // target2Transform.position = new Vector3(0, 0.5f, 0);
        // poleTransform.position = new Vector3(0, 0.5f, 0);
        // goalTransform.position = new Vector3(0, 0.5f, 0);
        bridgeDown.SetActive(false);
        bridgeUp.SetActive(true);
    }
}
