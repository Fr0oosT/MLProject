using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System.Collections;

public class MovingAgent : Agent
{
    public float moveSpeed = 5f;


    private Vector3 startPosition;

    public Transform opponentTransform;

    public Material winMaterial;
    public Material loseMaterial;
    public Material defaultMaterial;
    public MeshRenderer floorMeshRenderer;


    public override void Initialize()
    {
        startPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        opponentTransform.localPosition = new Vector3(
            Random.Range(-2f, +1.6f), 0.75f, Random.Range(+5f, +9f)
        );
        transform.localPosition = new Vector3(
            Random.Range(-2f, +1.6f), 0.75f, Random.Range(+1f, +4f)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);         // 3
        sensor.AddObservation(opponentTransform.localPosition); // 3
        // Total: 6 — set Vector Observation Space Size to 6
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;

        // Step penalty
        AddReward(-0.001f);

        // Distance reward — pull agent toward opponent every step
        // float dist = Vector3.Distance(transform.localPosition, opponentTransform.localPosition);
        // AddReward(-dist * 0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Keyboard.current.dKey.isPressed ? 1f : Keyboard.current.aKey.isPressed ? -1f : 0f;
        ca[1] = Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AddReward(2.0f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.CompareTag("Wall"))
        {
            AddReward(-2.0f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}