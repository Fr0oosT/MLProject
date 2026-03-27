using UnityEngine;
using Unity.MLAgents;

public class Opponent : MonoBehaviour
{
    [Header("Health")]
    public int StartingHealth = 100;
    private int CurrentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float strafeSpeed = 2f;
    public float attackDistance = 7f;
    public float tooCloseDistance = 3f;

    private float strafeDir = 1f;

    [Header("Shooting")]
    public Transform shootingPoint;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float reload = 1f;
    private float reloadTimer = 0f;
    private int damage = 100;

    [Header("Target")]
    public Transform agentTransform;

    private int stepsAlive = 0;

    void Start()
    {
        CurrentHealth = StartingHealth;
        strafeDir = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        stepsAlive++;
        reloadTimer -= Time.deltaTime;

        Vector3 toAgent = agentTransform.position - transform.position;
        toAgent.y = 0;
        float distance = toAgent.magnitude;

        // Face the agent
        transform.rotation = Quaternion.LookRotation(toAgent.normalized);

        // Movement logic
        if (distance > attackDistance)
        {
            // Close the gap aggressively
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else if (distance > tooCloseDistance)
        {
            // Strafe while maintaining pressure
            transform.position += transform.right * strafeDir * strafeSpeed * Time.deltaTime;
        }
        else
        {
            // Too close → back up
            transform.position -= transform.forward * moveSpeed * 0.5f * Time.deltaTime;
        }

        TryShoot();
    }

    private void TryShoot()
    {
        if (reloadTimer > 0f) return;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        Bullet b = bullet.GetComponent<Bullet>();
        b.damage = damage;
        b.targetLayerName = "Agent";
        b.opponentShooter = this;

        reloadTimer = reload;
    }

    public void GetShot(int damage, MLAgent shooter)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
            Die(shooter);
    }

    private void Die(MLAgent shooter)
    {
        Academy.Instance.StatsRecorder.Add("Enemy/Amount of Steps Alive", stepsAlive, StatAggregationMethod.Average);
        shooter.RegisterKill();
    }

    public void Respawn()
    {
        stepsAlive = 0;
        CurrentHealth = StartingHealth;
        strafeDir = Random.value > 0.5f ? 1f : -1f;
    }
}
