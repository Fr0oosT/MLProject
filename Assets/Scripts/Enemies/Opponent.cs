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

        int layerMask = 1 << LayerMask.NameToLayer("Agent");
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 1.0f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            AgentHealth agentHealth = hit.collider.GetComponent<AgentHealth>();
            if (agentHealth != null && agentHealth.gameObject.transform == agentTransform)
            {
                agentHealth.TakeDamage(damage, this);
            }
        }

        reloadTimer = reload;
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
        C3Agent shootingAgent = shooter as C3Agent;
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
