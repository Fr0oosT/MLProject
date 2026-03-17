using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ShootingAgent : Agent
{
    [Header("Shooting")]
    public Transform shootingPoint;
    public int minStepsBetweenShots = 50; // Minimum steps between shots
    public int damage = 100;

    private Vector3 startPosition;
    private Rigidbody rb;
    private bool shotAvailable = true;
    private int stepsUntilNextShotIsAvailable = 0;


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
        }

        shotAvailable = false;
        stepsUntilNextShotIsAvailable = minStepsBetweenShots;
    }

    // -------------------------------------------------------
    // Unity
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (!shotAvailable)
        {
            stepsUntilNextShotIsAvailable--;
            if (stepsUntilNextShotIsAvailable <= 0)
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
        rb.linearVelocity = Vector3.zero;
        shotAvailable = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(shotAvailable ? 1f : 0f); // 1 — can shoot?
        // Total: 1 — set Vector Observation Space Size to 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1)
            Shoot();

    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = Mouse.current.leftButton.isPressed ? 1 : 0;
    }

    public void RegisterKill()
    {
        AddReward(1.0f);
        EndEpisode();
    }
}