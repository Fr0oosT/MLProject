using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ShootingAgent : Agent
{
    [Header("Shooting")]
    public Transform shootingPoint;
    public float minTimeBetweenShots = 0.5f;
    public int damage = 20;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;


    private bool shotAvailable = true;
    private int stepsUntilNextShot = 0;

    private Vector3 startPosition;
    private Rigidbody rb;

    // -------------------------------------------------------
    // Shooting
    // -------------------------------------------------------
    private void Shoot()
    {
        if (!shotAvailable) return;

        int layerMask = 1 << LayerMask.NameToLayer("Opponent");
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;

        Debug.Log("Shot");
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Opponent opponent = hit.collider.GetComponent<Opponent>();
            if (opponent != null)
            {
                Debug.Log("Hit opponent!");
                opponent.GetShot(damage, this);
            }

            shotAvailable = false;
            stepsUntilNextShot = Mathf.RoundToInt(minTimeBetweenShots / Time.fixedDeltaTime);
        }
    }

    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (!shotAvailable)
        {
            stepsUntilNextShot--;
            if (stepsUntilNextShot <= 0)
                shotAvailable = true;
        }
    }

    // -------------------------------------------------------
    // ML-Agents
    // -------------------------------------------------------
    public override void Initialize()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        shotAvailable = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);               // 3 — facing direction
        sensor.AddObservation(transform.localPosition);         // 3 — position in arena
        sensor.AddObservation(rb.linearVelocity);               // 3 — current velocity
        sensor.AddObservation(shotAvailable ? 1f : 0f);         // 1 — can shoot?
        // Total: 10 — set Vector Observation Space Size to 10
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // --- Continuous actions ---
        float moveForward = actions.ContinuousActions[0]; // -1 to 1 (forward/back)
        float strafe     = actions.ContinuousActions[1]; // -1 to 1 (left/right)
        float rotate     = actions.ContinuousActions[2]; // -1 to 1 (turn)

        // Apply movement relative to agent's facing direction
        Vector3 move = (transform.forward * moveForward + transform.right * strafe)
                       * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Apply rotation
        float turn = rotate * rotateSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));

        // --- Discrete actions ---
        if (actions.DiscreteActions[0] == 1)
            Shoot();

        // Small per-step survival reward — encourages staying alive and active
        AddReward(0.001f);

        // Penalize standing still — encourages the agent to keep moving
        if (rb.linearVelocity.magnitude < 0.1f)
            AddReward(-0.002f);
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var ca = actionsOut.ContinuousActions;
    //     ca[0] = Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f;
    //     ca[1] = Keyboard.current.dKey.isPressed ? 1f : Keyboard.current.aKey.isPressed ? -1f : 0f;
    //     ca[2] = Mouse.current.delta.x.ReadValue() * 0.1f;

    //     var da = actionsOut.DiscreteActions;
    //     da[0] = Mouse.current.leftButton.isPressed ? 1 : 0;
    // }

    public void RegisterKill()
    {
        AddReward(1.0f);
        EndEpisode();
    }
}