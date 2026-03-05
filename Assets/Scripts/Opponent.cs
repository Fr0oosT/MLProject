using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
public class Opponent : MonoBehaviour
{
    public int StartingHealth = 100;
    private int CurrentHealth;
    private Vector3 StartPosition;

    void Start()
    {
        StartPosition = transform.position;
        CurrentHealth = StartingHealth;
    }

    public void GetShot(int damage, Agent shooter)
    {
       ApplyDamage(damage, shooter);
    }

    private void ApplyDamage(int damage, Agent shooter)
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
        transform.position = StartPosition;
    }

    private void OnMouseDown()
    {
        GetShot(StartingHealth, null);
    }
}