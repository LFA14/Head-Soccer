using UnityEngine;

public class MatchContext : MonoBehaviour
{
    public static MatchContext Instance;

    public enum MatchMode
    {
        None,
        QuickMatch,
        Tournament
    }

    public MatchMode currentMode = MatchMode.None;

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
    }
}