using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public enum MatchState { Countdown, Playing, MatchOver }
    public MatchState State { get; private set; }

    [Header("Settings")]
    public int countdownFrom = 3;

    // Who won — readable by UI scripts
    public bool playerWon { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        State = MatchState.Countdown;
        StartCoroutine(CountdownRoutine());
    }

    // -------------------------------------------------------
    // Called by your Health scripts when something dies
    // -------------------------------------------------------
    public void PlayerDied()
    {
        if (State != MatchState.Playing) return;
        playerWon = false;
        EndMatch();
    }

    public void EnemyDied()
    {
        if (State != MatchState.Playing) return;
        playerWon = true;
        EndMatch();
    }

    // -------------------------------------------------------
    // Internal
    // -------------------------------------------------------
    IEnumerator CountdownRoutine()
    {
        for (int i = countdownFrom; i > 0; i--)
        {
            MatchUI.Instance?.ShowCountdown(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        MatchUI.Instance?.ShowCountdown("GO!");
        yield return new WaitForSeconds(0.6f);

        MatchUI.Instance?.HideCountdown();
        State = MatchState.Playing;
    }

    public void EndMatch()
    {
        State = MatchState.MatchOver;
        ScoreManager.Instance?.RecordResult(playerWon);
        MatchUI.Instance?.ShowResult(playerWon);
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(3f); // wait before allowing restart or menu
        Scene currrentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currrentScene.buildIndex); // restart the match
    }

    // Call this from your result screen buttons
    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0); // adjust index to match your menu scene
    }
}