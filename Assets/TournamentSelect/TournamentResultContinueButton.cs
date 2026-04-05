using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentResultContinueButton : MonoBehaviour
{
    public string bracketSceneName = "TournamentBracketScene";
    public string mainMenuSceneName = "MenuScene";
    public string quickMatchResultSceneName = "QuickMatchResultScene";

    public void ContinueAfterTournamentResult()
    {
        if (IsQuickMatchResultScene())
        {
            if (MatchContext.Instance != null)
                MatchContext.Instance.SetMode(MatchContext.MatchMode.None);

            if (TournamentResultData.Instance != null)
                TournamentResultData.Instance.ClearResult();

            SceneManager.LoadScene(GetSafeMainMenuSceneName());
            return;
        }

        if (MatchContext.Instance != null && MatchContext.Instance.currentMode == MatchContext.MatchMode.QuickMatch)
        {
            MatchContext.Instance.SetMode(MatchContext.MatchMode.None);

            if (TournamentResultData.Instance != null)
                TournamentResultData.Instance.ClearResult();

            SceneManager.LoadScene(GetSafeMainMenuSceneName());
            return;
        }

        if (TournamentResultData.Instance == null)
        {
            SceneManager.LoadScene(GetSafeMainMenuSceneName());
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

    bool IsQuickMatchResultScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() &&
               activeScene.name == quickMatchResultSceneName;
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
