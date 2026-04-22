using UnityEngine;

public class MatchContext : MonoBehaviour
{
    public static MatchContext Instance;

    public enum MatchMode
    {
        None,
        QuickMatch,
        Tournament,
        Online
    }

    public MatchMode currentMode = MatchMode.None;
    public bool playerIsOnRightSide = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(MatchMode mode)
    {
        currentMode = mode;
        GameModeManager.SetOnlineMatch(mode == MatchMode.Online);
    }

    public void SetPlayerSide(bool isOnRightSide)
    {
        playerIsOnRightSide = isOnRightSide;
    }
}
