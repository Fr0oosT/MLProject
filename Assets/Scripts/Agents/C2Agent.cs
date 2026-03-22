using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class C2Agent : Agent
{
    private Rigidbody rb;
    private Vector3 startPosition;

    public Transform opponentTransform;
    private Rigidbody opponentRb;

    [Header("Visual Feedback")]
    public Material winMaterial;
    public Material loseMaterial;
    public MeshRenderer floorMeshRenderer;

    [Header("Shooting")]
    public Transform shootingPoint;
    private int damage = 100;
    private bool shotAvailable = true;
    private int stepsUntilNextShotIsAvailable = 0;

    [Header("Strafing")]
    private float strafeSpeed = 3f;
    private float rotationSpeed = 300f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        opponentRb = opponentTransform.GetComponent<Rigidbody>();
        startPosition = transform.localPosition;
    }

    private void Shoot()
    {
        if (!shotAvailable) return;

        int layerMask = 1 << LayerMask.NameToLayer("Opponent");
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Opponent opponent = hit.collider.GetComponent<Opponent>();
            if (opponent != null && opponent.transform == opponentTransform)
            {
                opponent.GetShot(damage, this);
                AddReward(0.3f);
            }
            else
            {
                AddReward(-0.1f); // Hit something else on opponent layer (e.g. wall)
            }
        }


        shotAvailable = false;
        stepsUntilNextShotIsAvailable = 50;
    }

    private void FixedUpdate()
    {
        if (!shotAvailable)
        {
            stepsUntilNextShotIsAvailable--;
            if (stepsUntilNextShotIsAvailable <= 0)
                shotAvailable = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        shotAvailable = true;

        transform.localPosition = startPosition;

        // Face the opponent at episode start
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);                         // 3
        sensor.AddObservation(opponentTransform.localPosition);                 // 3
        sensor.AddObservation(shotAvailable ? 1f : 0f);                         // 1

        // Opponent velocity
        Vector3 localOpponentVelocity = transform.InverseTransformDirection(opponentRb.linearVelocity);
        sensor.AddObservation(localOpponentVelocity); // 3
        // Debug.Log(localOpponentVelocity);

        // Agent facing direction relative to opponent
        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToOpponent);       // 3

        // Angle remaining to target, how far off its aim offset is
        float angleToOpponent = Vector3.SignedAngle(transform.forward, dirToOpponent, Vector3.up);
        sensor.AddObservation(angleToOpponent / 180f);                           // 1

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Shooting
        if (actions.DiscreteActions[0] == 1)
        {
            Shoot();
        }


        // Moving
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Debug.Log($"MoveX: {moveX}, MoveZ: {moveZ}");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.localPosition += move * strafeSpeed * Time.deltaTime;


        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToOpponent);
        float aimOffset = actions.ContinuousActions[2] * 30f;
        targetRotation *= Quaternion.Euler(0f, aimOffset, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Step penalty
        AddReward(-0.001f);


      


        // Range reward
        float dist = Vector3.Distance(transform.localPosition, opponentTransform.localPosition);
        if (dist >= 4f && dist <= 6f)
            AddReward(+0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Keyboard.current.dKey.isPressed ? 1f : Keyboard.current.aKey.isPressed ? -1f : 0f;
        ca[1] = Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f;
        ca[2] = Keyboard.current.eKey.isPressed ? 1f : Keyboard.current.qKey.isPressed ? -1f : 0f; // Q/E to rotate

        var da = actionsOut.DiscreteActions;
        da[0] = Mouse.current.leftButton.isPressed ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            floorMeshRenderer.material = loseMaterial;
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void RegisterKill()
    {
        AddReward(1.0f);
        floorMeshRenderer.material = winMaterial;
        EndEpisode();
    }
}