using UnityEngine;


public class EnemyBehavior4 : MonoBehaviour, IStateProvider
{
    public float speed = 1f;

    [SerializeField] private Transform playerTransform;

    public float[] currentState()
    {
        return Utility.vec3ToArr(transform.localPosition);
    }

    public float[] Dynamics()
    {
        return Utility.vec3ToArr(_dynamics());
    }

    void Update()
    {
        Vector3 movement = _dynamics();

        // Apply the movement
        transform.localPosition += movement * Time.deltaTime;
    }

    private Vector3 _dynamics()
    {
        var distance = playerTransform.localPosition - transform.localPosition;

        float moveX = Mathf.Clamp(distance.x, -1f, 1f);
        float moveZ = Mathf.Clamp(distance.z, -1f, 1f);
        var movement = new Vector3(moveX, 0f, moveZ) * speed;
        return movement;
    }
}
