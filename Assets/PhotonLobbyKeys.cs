using ExitGames.Client.Photon;
using Photon.Realtime;

public static class PhotonLobbyKeys
{
    public const string SelectedCharacterIndexKey = "SelectedCharacterIndex";
    public const string ReadyStateKey = "IsReady";

    public static Hashtable CreateLobbyProperties(int selectedCharacterIndex, bool isReady)
    {
        return new Hashtable
        {
            { SelectedCharacterIndexKey, selectedCharacterIndex },
            { ReadyStateKey, isReady }
        };
    }

    public static int GetSelectedCharacterIndex(Player player, int fallbackIndex, int maxCharacterCount)
    {
        if (player == null || maxCharacterCount <= 0)
        {
            return fallbackIndex;
        }

        if (player.CustomProperties != null &&
            player.CustomProperties.TryGetValue(SelectedCharacterIndexKey, out object rawValue) &&
            rawValue is int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < maxCharacterCount)
            {
                return selectedIndex;
            }
        }

        if (fallbackIndex < 0)
            return 0;

        return fallbackIndex >= maxCharacterCount ? maxCharacterCount - 1 : fallbackIndex;
    }

    public static bool GetReadyState(Player player)
    {
        if (player == null || player.CustomProperties == null)
        {
            return false;
        }

        if (player.CustomProperties.TryGetValue(ReadyStateKey, out object rawValue) && rawValue is bool readyState)
        {
            return readyState;
        }

        return false;
    }

    public static bool ContainsLobbyProperties(Hashtable changedProps)
    {
        if (changedProps == null)
        {
            return false;
        }

        return changedProps.ContainsKey(SelectedCharacterIndexKey) || changedProps.ContainsKey(ReadyStateKey);
    }

    public static string NormalizeRoomCode(string rawCode)
    {
        return string.IsNullOrWhiteSpace(rawCode)
            ? string.Empty
            : rawCode.Trim().ToUpperInvariant();
    }
}
