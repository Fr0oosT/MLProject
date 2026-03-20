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
    private Vector3 opponentPreviousPosition;

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
    private float strafeSpeed = 6f;
    private float rotationSpeed = 500f;

    private int facingAwaySteps = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
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
                AddReward(-0.1f);
            }
        }
        else
        {
            AddReward(-0.15f); // Miss penalty
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

        // opponentTransform.localPosition = new Vector3(
        //     Random.Range(-2f, 1.6f), 0.75f, Random.Range(7f, 9f)
        // );
        transform.localPosition = new Vector3(
            Random.Range(-2f, 1.6f), 0.75f, Random.Range(1f, 3f)
        );

        // Face the opponent at episode start
        transform.rotation = Quaternion.identity;

        opponentPreviousPosition = opponentTransform.localPosition;

        facingAwaySteps = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);                         // 3
        sensor.AddObservation(opponentTransform.localPosition);                 // 3
        sensor.AddObservation(shotAvailable ? 1f : 0f);                         // 1

        // Opponent velocity — critical for C3 tracking
        Vector3 opponentVelocity = (opponentTransform.localPosition - opponentPreviousPosition) / Time.fixedDeltaTime;
        sensor.AddObservation(opponentVelocity);                                // 3

        // Agent's facing direction relative to opponent
        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToOpponent);                                   // 3

        // How well the agent is facing the opponent (1 = perfect, -1 = facing away)
        sensor.AddObservation(Vector3.Dot(transform.forward, dirToOpponent));   // 1

        opponentPreviousPosition = opponentTransform.localPosition;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Shooting
        if (actions.DiscreteActions[0] == 1)
        {
            AddReward(0.01f); // Small reward for attempting to shoot, encourages learning the timing
            Shoot();
        }


        // Strafing
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * strafeSpeed * Time.deltaTime;

        // Rotation — agent turns to track opponent
        float rotateY = actions.ContinuousActions[2];
        transform.Rotate(0f, rotateY * rotationSpeed * Time.deltaTime, 0f);

        // Step penalty
        AddReward(-0.001f);


        // Reward for facing the opponent — encourages active tracking
        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        float facingDot = Vector3.Dot(transform.forward, dirToOpponent);
        AddReward(facingDot * 0.001f);

        if (facingDot < 0f)
        {
            facingAwaySteps++;
            AddReward(-0.05f);
            if (facingAwaySteps > 50)
            {
                AddReward(-1f);
                floorMeshRenderer.material = loseMaterial;
                EndEpisode();
            }
        }
        else
        {
            facingAwaySteps = 0;
            if      (facingDot < 0.8f  && actions.DiscreteActions[0] == 1) AddReward(-0.04f);  // Facing sideways while shooting
            else if (facingDot > 0.95f  && actions.DiscreteActions[0] == 1) AddReward(+0.05f);  // Well aimed and shooting
            else if (facingDot > 0.8f  && Mathf.Abs(actions.ContinuousActions[2]) > 0.05f)  AddReward(-0.01f);      // Spinning when aimed
        }


                // Range reward — fixed logic
        float dist = Vector3.Distance(transform.localPosition, opponentTransform.localPosition);
        if (dist >= 4f && dist <= 6f && facingDot > 0.7f)
            AddReward(+0.01f);
        else
            AddReward(-0.01f);
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