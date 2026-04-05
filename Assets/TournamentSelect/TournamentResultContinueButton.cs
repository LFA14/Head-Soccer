using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentResultContinueButton : MonoBehaviour
{
    public string bracketSceneName = "TournamentBracketScene";
    public string mainMenuSceneName = "MenuScene";

    public void ContinueAfterTournamentResult()
    {
        if (TournamentResultData.Instance == null)
        {
            SceneManager.LoadScene(bracketSceneName);
            return;
        }

        if (TournamentResultData.Instance.wonTournament)
        {
            if (MatchContext.Instance != null)
                MatchContext.Instance.SetMode(MatchContext.MatchMode.None);

            SceneManager.LoadScene(bracketSceneName);
        }
        else
        {
            SceneManager.LoadScene(bracketSceneName);
        }
    }

    string GetSafeMainMenuSceneName()
    {
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName) &&
            Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            return mainMenuSceneName;
        }

        return "MenuScene";
    }
}
