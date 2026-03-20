using UnityEngine;

public class OpponentStrafe : MonoBehaviour
{
    [Header("Strafe Settings")]
    public float speed = 1.5f;
    public float range = 2f; // How far left/right from spawn point

    private Vector3 origin;

    private void Start()
    {
        origin = transform.localPosition;
    }

    public void ResetPosition()
    {
        transform.localPosition = origin;
    }

    private void Update()
    {
        float x = origin.x + Mathf.Sin(Time.time * speed) * range;
        transform.localPosition = new Vector3(x, origin.y, origin.z);
    }
}