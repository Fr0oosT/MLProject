using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int PlayerWins { get; private set; }
    public int EnemyWins  { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persists across scenes
    }

    public void RecordResult(bool playerWon)
    {
        if (playerWon) PlayerWins++;
        else           EnemyWins++;
    }

    public void ResetScore()
    {
        PlayerWins = 0;
        EnemyWins  = 0;
    }
}