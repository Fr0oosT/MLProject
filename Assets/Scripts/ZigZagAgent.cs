using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class ZigZagAgent : Agent
{
    [Header("Shooting")]
    public Transform shootingPoint;
    public float minTimeBetweenShots = 0.5f;
    public int damage = 20;

    [Header("Z Pattern Movement")]
    public float moveSpeed = 4f;
    public float zWidth = 3f;
    public float zHeight = 2f;

    private Vector3[] zPoints;
    private int currentPoint;
    private int direction = 1; // 1 = forward, -1 = reverse

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

        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Opponent opponent = hit.collider.GetComponent<Opponent>();
            if (opponent != null)
                opponent.GetShot(damage, this);
                AddReward(0.3f); //Reward for hitting opponent
        }
        else
        {
            // Debug.Log("Ray hit nothing");
            AddReward(-0.1f); // Penalize missed shots
        }

        shotAvailable = false;
        stepsUntilNextShot = Mathf.RoundToInt(minTimeBetweenShots / Time.fixedDeltaTime);
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

        // Move toward current point
        Vector3 targetPoint = zPoints[currentPoint];
        transform.position = Vector3.MoveTowards(
            transform.position, targetPoint, moveSpeed * Time.fixedDeltaTime
        );

        // Always face north
        transform.rotation = Quaternion.identity;

        // Advance to next point when close enough
        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
        {
            int nextPoint = currentPoint + direction;

            // Reverse direction at either end
            if (nextPoint >= zPoints.Length || nextPoint < 0)
            {
                direction *= -1;
                nextPoint = currentPoint + direction;
            }

            currentPoint = nextPoint;
        }
    }

    // -------------------------------------------------------
    // ML-Agents
    // -------------------------------------------------------
    public override void Initialize()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        BuildZPoints();
    }

    private void BuildZPoints()
    {
        // Z pattern:
        // Index 0: bottom right (start)
        // Index 1: bottom left
        // Index 2: top right  (diagonal cross)
        // Index 3: top left
        zPoints = new Vector3[]
        {
            startPosition + new Vector3( zWidth, 0f, -zHeight),  // bottom right
            startPosition + new Vector3(-zWidth, 0f, -zHeight),  // bottom left
            startPosition + new Vector3( zWidth, 0f,  zHeight),  // top right
            startPosition + new Vector3(-zWidth, 0f,  zHeight),  // top left
        };
        currentPoint = 0;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        shotAvailable = true;
        currentPoint = 0;
        direction = 1;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);               // 3
        sensor.AddObservation(transform.localPosition);         // 3
        sensor.AddObservation(rb.linearVelocity);               // 3
        sensor.AddObservation(shotAvailable ? 1f : 0f);         // 1
        // Total: 10
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement handled in FixedUpdate via Z pattern
        if (actions.DiscreteActions[0] == 1)
        {
             Shoot();
             
        }
           
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