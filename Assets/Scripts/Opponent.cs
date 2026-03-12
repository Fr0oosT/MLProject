using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
public class Opponent : MonoBehaviour
{
    public int StartingHealth = 100;
    private int CurrentHealth;

    [Header("Left-Right Movement")]
    public float moveSpeed = 4f;
    public float zWidth = 3f;
    private Vector3 StartPosition;

    [Header("Pattern System")]
    private Vector3[] zPoints;
    // private int currentPoint;
    // private int direction = 1; // 1 = forward, -1 = reverse

    private Vector3 randomPoint;

    void Start()
    {
        StartPosition = transform.position;
        CurrentHealth = StartingHealth;
    }

    private void TeleportToRandom()
    {
        float randomX = Random.Range(-zWidth, zWidth);
        transform.position = StartPosition + new Vector3(randomX, 0f, 0f);
    }       

    public void GetShot(int damage, Agent shooter)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Die(shooter);
        }
    }

    private void Die(Agent shooter)
    {
        Debug.Log("Opponent died!");
        ShootingAgent shootingAgent = shooter as ShootingAgent;
        if (shootingAgent != null)
        {
            shootingAgent.RegisterKill();
        }
        Respawn();
    }

    private void Respawn()
    {
        CurrentHealth = StartingHealth;
        
        TeleportToRandom();
    }

    private void OnMouseDown()
    {
        GetShot(StartingHealth, null);
    }
}