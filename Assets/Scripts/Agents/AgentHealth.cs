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

    public void TakeDamage(int amount, Opponent shooter)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die(shooter);
        }
    }

    private void Die(Opponent shooter)
    {
        // Negative reward for dying
        agent.AddReward(-1f);

        // End the episode
        agent.EndEpisode();
    }
}
