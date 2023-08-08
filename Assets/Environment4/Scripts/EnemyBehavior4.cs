using Unity.MLAgents.Actuators;
using UnityEngine;


public class EnemyBehavior4 : MonoBehaviour, IDynamicsProvider
{
    public float maxSpeed = 1f;
    public float minSpeed = 1f;
    [HideInInspector]
    public float speed = 1f;

    [SerializeField] private Transform playerTransform;

    public float[] x()
    {
        return Utility.vec3ToArr(transform.localPosition);
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

    public float[] dxdt(ActionBuffers action)
    {
        return Utility.vec3ToArr(_dynamics());
    }
    public float[] delta_x(ActionBuffers action, float delta_t)
    {
        return Utility.vec3ToArr(_dynamics() * delta_t);
    }
}
