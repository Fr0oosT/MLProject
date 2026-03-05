using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using UnityEngine.InputSystem;


public class ShootingAgent : Agent
{
    public Transform shootingPoint;
    public float minTimeBetweenShots = 0.5f;
    public int damage = 20;

    private bool shotAvailable = true;
    private int stepsUntilNextShot = 0;

    private Vector3 startPosition;
    private Rigidbody rb;

    private void Shoot()
    {
        if (!shotAvailable)
        {
            return;
        }
        int layerMask = 1 << LayerMask.NameToLayer("Opponent");

        
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;

        Debug.Log(message: "Shot");
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Opponent opponent = hit.collider.GetComponent<Opponent>();
            if (opponent != null)
            {
                opponent.GetShot(damage, this); // Example damage value
            }

            shotAvailable = false;
            stepsUntilNextShot = Mathf.RoundToInt(minTimeBetweenShots / Time.fixedDeltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!shotAvailable)
        {
            stepsUntilNextShot--;
            if (stepsUntilNextShot <= 0)
            {
                shotAvailable = true;
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 1)
        {
            Shoot();
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        shotAvailable = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);
    }

    public override void Initialize()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

public override void Heuristic(in ActionBuffers actionsOut)
{
    var discreteActionsOut = actionsOut.DiscreteActions;
    discreteActionsOut[0] = Mouse.current.leftButton.isPressed ? 1 : 0;
}

    public void RegisterKill()
    {
        AddReward(1.0f); 
        EndEpisode(); 

    }
}