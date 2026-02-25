using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            // Handle bullet collision (e.g., reduce health, destroy enemy, etc.)
            Debug.Log("Enemy hit by a bullet!");
            Destroy(gameObject); // Example: destroy the enemy
        }
    }
}
