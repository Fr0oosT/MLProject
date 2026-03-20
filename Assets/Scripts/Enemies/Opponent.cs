using UnityEngine;
using Unity.MLAgents;

public class Opponent : MonoBehaviour
{
    public int StartingHealth = 100;
    private int CurrentHealth;

    private OpponentStrafe OpponentStrafe;

    // private Vector3 startPosition;
    void Start()
    {
        // startPosition = transform.position;
        CurrentHealth = StartingHealth;

        OpponentStrafe = GetComponent<OpponentStrafe>();

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
        C2Agent shootingAgent = shooter as C2Agent;
        if (shootingAgent != null)
        {
            shootingAgent.RegisterKill();
        }
        Respawn();
    }

    private void Respawn()
    {
        CurrentHealth = StartingHealth;
        
        // transform.position = startPosition;
        OpponentStrafe.ResetPosition();
    }

    private void OnMouseDown()
    {
        GetShot(StartingHealth, null);
    }
}