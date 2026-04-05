using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchEndNextButton : MonoBehaviour
{
    public string quickMatchResultScene = "QuickMatchResultScene";
    public string tournamentResultScene = "TournamentResultScene";

    public void GoToResultScene()
    {
        CountdownManager matchManager = FindObjectOfType<CountdownManager>();
        if (matchManager != null)
            matchManager.FinalizeMatchResultIfNeeded();

        if (MatchContext.Instance == null)
        {
            Debug.LogWarning("MatchContext missing, loading quick match result.");
            SceneManager.LoadScene(quickMatchResultScene);
            return;
        }

        Debug.Log("Current match mode: " + MatchContext.Instance.currentMode);

        if (MatchContext.Instance.currentMode == MatchContext.MatchMode.Tournament)
            SceneManager.LoadScene(tournamentResultScene);
        else
            SceneManager.LoadScene(quickMatchResultScene);
    }
}
