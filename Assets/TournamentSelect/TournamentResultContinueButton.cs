using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentResultContinueButton : MonoBehaviour
{
    public string bracketSceneName = "TournamentBracketScene";
    public string mainMenuSceneName = "MainMenu";

    public void ContinueAfterTournamentResult()
    {
        if (TournamentResultData.Instance == null)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        if (TournamentResultData.Instance.wonTournament)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else if (TournamentResultData.Instance.qualified)
        {
            SceneManager.LoadScene(bracketSceneName);
        }
        else
        {
            SceneManager.LoadScene(bracketSceneName);
        }
    }
}