using UnityEngine;

public class OpponentStrafe : MonoBehaviour
{
    [Header("Strafe Settings")]
    private float speed = 1f;
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
        // transform.localPosition = origin;
        // rb.linearVelocity = Vector3.zero;
        // direction = 1f; // Reset direction to default
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