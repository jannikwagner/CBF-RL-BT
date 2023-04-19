using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    public Transform player;
    public Transform button;
    public Transform buttonTrigger;
    public Transform goalTrigger;
    public Transform goal;
    public GameObject bridgeDown;
    public GameObject bridgeUp;

    public float groundY = 0f;
    public float elevatedGroundY = 4f;
    public readonly float width = 40f;

    float part1 = 0.25f;
    float part2 = 0.25f;
    float part3 = 0.25f;
    float part4 = 0.25f;
    float playerScale = 1f;
    float buttonHeight = 0.0002f;
    float margin = 1f;
    float podiumBredth = 10f;

    float x0;
    float x1;
    float x2;
    float x3;
    float x4;
    float height;

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
        return Vector3.Distance(buttonTrigger.position, button.position) < 1.0f;
    }

    public bool win()
    {
        return Vector3.Distance(goalTrigger.position, goal.position) < 1.0f;
    }

    public bool TargetUp()
    {
        return buttonTrigger.position.y >= elevatedGroundY + 0.5f;
    }

    public float DistanceTargetUp()
    {
        if (TargetUp())
        {
            return 0f;
        }
        else
        {
            return buttonTrigger.position.x - x1 - playerScale;
        }
    }

    public void Initialize()
    {
        x0 = -width / 2;
        x1 = x0 + width * part1;
        x2 = x1 + width * part2;
        x3 = x2 + width * part3;
        x4 = x3 + width * part4;
        height = elevatedGroundY - groundY;

        float minX = x0 + margin + playerScale;
        float maxX = x4 - margin - playerScale;
        float maxXTarget = x3 - margin - playerScale;
        float minZ = -width / 2 + margin + playerScale;
        float maxZ = width / 2 - margin - playerScale;
        float x1WithScale = x1 - playerScale;
        float playerY = elevatedGroundY + playerScale / 2;
        float buttonY = elevatedGroundY + buttonHeight / 2;
        float podiumZ1 = -podiumBredth / 2;
        float podiumZ2 = podiumBredth / 2;

        player.localPosition = new Vector3(-15, playerY, -2);
        player.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        buttonTrigger.localPosition = new Vector3(-8, playerY, -2);
        buttonTrigger.localRotation = Quaternion.Euler(0, 0, 0);
        goalTrigger.localPosition = new Vector3(-4, playerY, 0);

        button.localPosition = new Vector3(-2, buttonY, -2);

        float buttonMaxX = -8;
        float buttonTiltStartX = -20;
        float buttonSmallMaxZ = -2;
        float buttonSmallMinZ = -6;

        // player.localPosition = new Vector3(Random.Range(minX, maxX), playerY, Random.Range(minZ, maxZ));
        // target.localPosition = new Vector3(Random.Range(minX, maxXTarget), playerY, Random.Range(minZ, maxZ));
        // buttonTrigger.localPosition = Utility.SamplePosition(minX, maxXTarget, minZ, maxZ, playerY, playerY, 2f, new Vector3[] { button.localPosition });
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
