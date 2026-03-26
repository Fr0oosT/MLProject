using UnityEngine;
using Unity.MLAgents;

public class AgentHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;

    private MLAgent agent;

    private void Awake()
    {
        agent = GetComponent<MLAgent>();
    }

    public void ResetHealth()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            DieByEnemy();
        }
    }

    private void DieByEnemy()
    {
        agent.RegisterDeathByEnemy();
    }

}
