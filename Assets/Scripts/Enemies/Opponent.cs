using UnityEngine;
using Unity.MLAgents;

public class Opponent : MonoBehaviour
{
    public int StartingHealth = 100;
    private int CurrentHealth;

    private Rigidbody rb;
    private OpponentStrafe opponentStrafe;

    [Header("Shooting")]
    public Transform shootingPoint;
    public int damage = 20;
    public float reload = 1f; // how long the opponent has to wait between shots
    private float reloadTimer = 0f; // countdown until next shot

    [Header("Projectile")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;


    [Header("Target")]
    public Transform agentTransform; 

    void Start()
    {
        CurrentHealth = StartingHealth;
        rb = GetComponent<Rigidbody>();
        opponentStrafe = GetComponent<OpponentStrafe>();
    }

    private void Update()
    {
        reloadTimer -= Time.deltaTime;

        // Face the agent
        Vector3 dir = agentTransform.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        TryShoot();
    }

    private void TryShoot()
    {
        if (reloadTimer > 0f) return;

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);

        // Give it velocity
        Rigidbody brb = bullet.GetComponent<Rigidbody>();
        brb.linearVelocity = shootingPoint.forward * bulletSpeed;

        // Pass target + shooter to bullet
        Bullet b = bullet.GetComponent<Bullet>();
        b.target = agentTransform;
        b.damage = damage;
        b.targetLayerName = "Agent";


        // Reset reload timer
        reloadTimer = reload;

        // Cleanup
        Destroy(bullet, 2f);
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
        MLAgent shootingAgent = shooter as MLAgent;
        if (shootingAgent != null)
        {
            shootingAgent.RegisterKill();
        }
    }

    public void Respawn()
    {
        CurrentHealth = StartingHealth;
        opponentStrafe.ResetPosition();
    }
}
