using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    [Header("Combat")]
    public float fireRate = 1f;
    public float bulletSpeed = 10f;
    public float shootRange = 8f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Transform player;
    private float nextFireTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // BUG FIX: condition was inverted — enemy only acted when NOT playing
        if (MatchManager.Instance?.State != MatchManager.MatchState.Playing) return;
        if (player == null) return;

        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        transform.LookAt(player);

        // Cache sqrMagnitude instead of Vector3.Distance to avoid a sqrt call
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        if (sqrDist <= shootRange * shootRange && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position + firePoint.forward * 1f,
            firePoint.rotation);

        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = firePoint.forward * bulletSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Destroy(gameObject);
            MatchManager.Instance.EnemyDied();
        }
    }
}