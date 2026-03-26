using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class MLAgent : Agent
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

    public Transform bulletSpawnPoint;
    private int damage = 100;
    private bool shotAvailable = true;
    private int stepsUntilNextShotIsAvailable = 0;

    [Header("Projectile")]
    public GameObject bulletPrefab;

    [Header("Strafing")]
    private float strafeSpeed = 3f;
    private float rotationSpeed = 300f;

    private AgentHealth health;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        opponentRb = opponentTransform.GetComponent<Rigidbody>();
        startPosition = transform.localPosition;
        health = GetComponent<AgentHealth>();
    }

    private void Shoot()
    {
        if (!shotAvailable) return;

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        // Debug.Log("Bullet Spawned at " + bulletSpawnPoint.position);

        // Pass shooter + target to bullet
        Bullet b = bullet.GetComponent<Bullet>();
        b.shooter = this;
        b.damage = damage;
        b.targetLayerName = "Opponent";


        // Cooldown
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

        health.ResetHealth();

        opponentTransform.GetComponent<Opponent>().Respawn();
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);                         // 3
        sensor.AddObservation(opponentTransform.localPosition);                 // 3
        sensor.AddObservation(shotAvailable ? 1f : 0f);                         // 1

        // Opponent velocity
        Vector3 localOpponentVelocity = transform.InverseTransformDirection(opponentRb.linearVelocity);
        sensor.AddObservation(localOpponentVelocity); // 3


        // Agent facing direction relative to opponent
        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToOpponent);       // 3

        // Angle remaining to target, how far off its aim offset is
        float angleToOpponent = Vector3.SignedAngle(transform.forward, dirToOpponent, Vector3.up);
        sensor.AddObservation(angleToOpponent / 180f);                           // 1

        // Line of sight check
        bool inSight = IsOpponentInSight();
        sensor.AddObservation(inSight ? 1f : 0f); // 1

        // Last seen position (world space relative to the agent)
        Vector3 LastSeen = inSight ? transform.InverseTransformPoint(opponentTransform.position) : transform.InverseTransformPoint(opponentTransform.position + Vector3.zero);
        sensor.AddObservation(LastSeen); // 3

        sensor.AddObservation(health.currentHealth / 100f); // 1
    }

    private bool IsOpponentInSight()
    {
        Vector3 dir = (opponentTransform.position - shootingPoint.position).normalized;
        if (Physics.Raycast(shootingPoint.position, dir, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Opponent")))
        {
            if (hit.collider.transform == opponentTransform)
                return true;
        }
        return false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Shooting
        if (actions.DiscreteActions[0] == 1)
        {
            Shoot();
        }

        AddReward(+0.0005f); // Small reward for staying alive each step


        // Moving
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

  

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.localPosition += move * strafeSpeed * Time.deltaTime;


        Vector3 dirToOpponent = (opponentTransform.localPosition - transform.localPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToOpponent);
        float aimOffset = actions.ContinuousActions[2] * 30f;
        targetRotation *= Quaternion.Euler(0f, aimOffset, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Step penalty
        AddReward(-0.001f);

        bool inSight = IsOpponentInSight();

        if (inSight)
            AddReward(+0.0005f); // Reward for having opponent in sight
        
        if(actions.DiscreteActions[0] == 1 && !inSight)
            AddReward(-0.05f); // Penalty when shooting blindly

        if(IsOpponentInSight() && !shotAvailable)
            AddReward(-0.02f); // Penalty for trying to shoot while on cooldown

        if(moveX != 0f || moveZ != 0f)
            AddReward(-0.005f); // Additional reward for shooting while opponent is in sight
        // Range reward

        float dist = Vector3.Distance(transform.localPosition, opponentTransform.localPosition);
        if (dist >= 4f && dist <= 6f)
            AddReward(+0.02f);
        
        if (dist < 1.0f)
            AddReward(-0.05f); // Too close is bad

        Vector3 toOpponent = (opponentTransform.position - transform.position).normalized;
        float perpendicular = Vector3.Dot(move.normalized, Vector3.Cross(toOpponent, Vector3.up));
        if (Mathf.Abs(perpendicular) > 0.7f) // strong sideways movement
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
            Academy.Instance.StatsRecorder.Add("Agent/Wall Deaths", 1, StatAggregationMethod.Sum);
            floorMeshRenderer.material = loseMaterial;
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void RegisterDeathByEnemy()
    {
        Debug.Log("Agent was killed by enemy.");
        Academy.Instance.StatsRecorder.Add("Enemy/Kills", 1, StatAggregationMethod.Sum);
        AddReward(-1f);
        floorMeshRenderer.material = loseMaterial;
        EndEpisode();
    }

    public void RegisterKill()
    {
        Academy.Instance.StatsRecorder.Add("Agent/Kills", 1, StatAggregationMethod.Sum);

        AddReward(3.0f);
        floorMeshRenderer.material = winMaterial;
        EndEpisode();
    }
}