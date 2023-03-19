using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior4 : MonoBehaviour
{
    public float speed = 1f;

    [SerializeField] private Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var distance = playerTransform.localPosition - transform.localPosition;

        float moveX = Mathf.Clamp(distance.x, -1f, 1f);
        float moveZ = Mathf.Clamp(distance.z, -1f, 1f);
        var movement = new Vector3(moveX, 0f, moveZ);

        // Apply the movement
        transform.localPosition += movement * speed * Time.deltaTime;
    }
}
