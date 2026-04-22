using UnityEngine;

public static class GameModeManager
{
    public static bool IsOnlineMatch { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        IsOnlineMatch = false;
    }

    public static void SetOnlineMatch(bool isOnlineMatch)
    {
        IsOnlineMatch = isOnlineMatch;
    }
}
