using UnityEngine;
using TMPro;

public class MatchUI : MonoBehaviour
{
    public static MatchUI Instance { get; private set; }

    public GameObject countdownPanel;
    public TMP_Text   countdownText;

    public GameObject winPanel;
    public GameObject losePanel;


    public TMP_Text scoreText; // e.g. "2 - 1"

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Make sure result panels start hidden
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // Called by MatchManager during countdown
    public void ShowCountdown(string value)
    {
        countdownPanel.SetActive(true);
        countdownText.text = value;
    }

    public void HideCountdown()
    {
        countdownPanel.SetActive(false);
    }

    // Called by MatchManager when match ends
    public void ShowResult(bool playerWon)
    {
        if (playerWon) winPanel.SetActive(true);
        else           losePanel.SetActive(true);

        RefreshScore();
    }

    public void RefreshScore()
    {
        if (scoreText == null || ScoreManager.Instance == null) return;
        scoreText.text = $"{ScoreManager.Instance.PlayerWins} - {ScoreManager.Instance.EnemyWins}";
    }
}