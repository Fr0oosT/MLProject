using UnityEngine;

public class OpponentStrafe : MonoBehaviour
{
    [Header("Strafe Settings")]
    private float speed = 1.5f;
    private float range = 2f; // How far left/right from spawn point

    private float direction = 1f;

    private Rigidbody rb;

    private Vector3 origin;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = transform.localPosition;
    }

    public void ResetPosition()
    {
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        transform.localPosition = origin + randomOffset;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        direction = Random.value > 0.5f ? 1f : -1f;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Vector3.right * direction * speed;

        float distFromOrigin = transform.localPosition.x - origin.x;
        if (distFromOrigin >= range)
            direction = -1f;
        else if (distFromOrigin <= -range)
            direction = 1f;


    }

}