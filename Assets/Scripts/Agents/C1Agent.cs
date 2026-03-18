using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System.Collections;

public class C1Agent : Agent
{
    private Rigidbody rb;

    private Vector3 startPosition;

    public Transform opponentTransform;

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

        Debug.Log("Shot");
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Opponent opponent = hit.collider.GetComponent<Opponent>();
            if (opponent != null && opponent.transform == opponentTransform)
            {
                Debug.Log("Hit opponent!");
                opponent.GetShot(damage, this);
                AddReward(0.3f); //Reward for hitting opponent
            }
            else
            {
                AddReward(-0.1f); // Penalize hitting wrong target
            }
        }

        shotAvailable = false;
        stepsUntilNextShotIsAvailable = 50; // Cooldown of 50 steps between shots
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


        opponentTransform.localPosition = new Vector3(
            Random.Range(-2f, +1.6f), 0.75f, Random.Range(+7f, +9f)
        );
            transform.localPosition = new Vector3(
            Random.Range(-2f, +1.6f), 0.75f, Random.Range(+1f, +3f)
        );

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);         // 3
        sensor.AddObservation(opponentTransform.localPosition); // 3
        sensor.AddObservation(shotAvailable ? 1f : 0f);         // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1)
            Shoot();
        
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0f, moveZ) * strafeSpeed * Time.deltaTime;

        // Step penalty
        AddReward(-0.001f);

        float distanceToOpponent = Vector3.Distance(transform.localPosition, opponentTransform.localPosition);
        if (distanceToOpponent < 6f && distanceToOpponent > 4f)
        {
            AddReward(+0.01f);
        }
        else if (distanceToOpponent > 4f)
        {
            AddReward(-0.01f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Keyboard.current.dKey.isPressed ? 1f : Keyboard.current.aKey.isPressed ? -1f : 0f;
        ca[1] = Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f;
        
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