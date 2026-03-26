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
    public Transform bulletSpawnPoint;
    private int damage = 100;
    public float reload = 1f; // how long the opponent has to wait between shots
    private float reloadTimer = 0f; // countdown until next shot

    [Header("Projectile")]
    public GameObject bulletPrefab;



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
        Vector3 dir = (agentTransform.position - shootingPoint.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        TryShoot();
    }

    private void TryShoot()
    {
        if (reloadTimer > 0f) return;

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Pass target + shooter to bullet
        Bullet b = bullet.GetComponent<Bullet>();
        b.damage = damage;
        // b.bulletSpeed = 40f;
        b.targetLayerName = "Agent";
        b.opponentShooter = this;


        // Reset reload timer
        reloadTimer = reload;
    }

    public void GetShot(int damage, MLAgent shooter)
    {
        // Debug.Log($"GetShot!");
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Die(shooter);
        }
    }

    private void Die(MLAgent shooter)
    {
        shooter.RegisterKill();
    }


    public void Respawn()
    {
        CurrentHealth = StartingHealth;
        opponentStrafe.ResetPosition();
    }
}
