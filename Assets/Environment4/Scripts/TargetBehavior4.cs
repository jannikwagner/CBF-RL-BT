using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehavior4 : MonoBehaviour
{
    public float speed = 1f;
    private float velX = 0f;
    private float velZ = 0f;

    void Start()
    {
        velX = Random.Range(-1f, 1f);
        velZ = Random.Range(-1f, 1f);
    }


    // Update is called once per frame
    void Update()
    {
        float forceX = Random.Range(-1f, 1f);
        float forceY = Random.Range(-1f, 1f);
        velX = Mathf.Clamp(velX + forceX * 0.1f, -1f, 1f);
        velZ = Mathf.Clamp(velZ + forceY * 0.1f, -1f, 1f);
        if (transform.localPosition.x + velX <= -8 || transform.localPosition.x + velX >= 8)
        {
            velX = -velX;
        }
        if (transform.localPosition.z + velZ <= -8 || transform.localPosition.z + velZ >= 8)
        {
            velZ = -velZ;
        }
        var movement = new Vector3(velX, 0f, velZ);

        // Apply the movement
        transform.localPosition += movement * speed * Time.deltaTime;
    }
}
