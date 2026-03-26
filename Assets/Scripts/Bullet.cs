using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 100;
    public MLAgent shooter;              // null if shooter is opponent
    public Opponent opponentShooter;     // null if shooter is agent
    public float bulletSpeed = 10f;
    public float lifetime = 3f;

    public string targetLayerName;       // "Opponent" or "Agent"

    private void Start()
    {
        // Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Layer check
        if (other.gameObject.layer != LayerMask.NameToLayer(targetLayerName))
        {
            Destroy(gameObject);
            return;
        }

        // Wall check
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // Bullet vs bullet
        if (other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
            return;
        }

        // Apply damage
        if (targetLayerName == "Opponent")
        {
            Opponent opp = other.GetComponent<Opponent>();
            if (opp != null)
            {
                opp.GetShot(damage, shooter);
                shooter?.AddReward(+3f);
            }
        }
        else if (targetLayerName == "Agent")
        {
            AgentHealth agentHealth = other.GetComponent<AgentHealth>();
            if (agentHealth != null)
            {
                agentHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
