using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 100;
    public Transform target;     // who this bullet is meant to hit
    public MLAgent shooter;   // null if shooter is opponent
    public float lifetime = 3f;

    public string targetLayerName; // "Opponent" for agent bullets, "Agent" for opponent bullets
    private bool armed = false; // short delay before bullet can deal damage, to prevent self-collision on spawn
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.05f);
        armed = true;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!armed) return;
        // 1. Check correct layer
        if (collision.collider.gameObject.layer != LayerMask.NameToLayer(targetLayerName))
        {
            Destroy(gameObject);
            return;
        }

        // 2. Check correct target transform
        if (collision.collider.transform != target)
        {
            Destroy(gameObject);
            return;
        }

        if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit wall and was destroyed.");
            Destroy(gameObject);
            return;
        }


        // 3. Apply damage depending on target type
        if (targetLayerName == "Opponent")
        {
            Opponent opp = collision.collider.GetComponent<Opponent>();
            if (opp != null)
            {
                opp.GetShot(damage, shooter);
                shooter?.AddReward(+3f); 
            }
        }
        else if (targetLayerName == "Agent")
        {
            AgentHealth agentHealth = collision.collider.GetComponent<AgentHealth>();
            if (agentHealth != null)
            {
                agentHealth.TakeDamage(damage, null); 
            }
        }

        Destroy(gameObject);
    }
}
