using System;

public class FakeLobbyManager
{
    public const string WaitingForPlayerStatus = "Waiting for player...";
    public const string OpponentJoinedStatus = "Opponent joined!";
    public const string ConnectedStatus = "Connected!";
    public const string ReadyStatus = "Ready!";
    public const string NotReadyStatus = "Not Ready";
    public const string BothPlayersReadyStatus = "Both players ready";
    public const string OpponentReadyStatus = "Opponent ready. You are not ready.";
    public const string WaitingForOpponentReadyStatus = "Ready! Waiting for opponent...";
    public const string WaitingForPlayerWhileReadyStatus = "Ready! Waiting for player...";

    private readonly Random random = new Random();

    public bool InRoom { get; private set; }
    public bool IsHost { get; private set; }
    public bool OpponentPresent { get; private set; }
    public bool LocalReady { get; private set; }
    public bool OpponentReady { get; private set; }
    public string LobbyCode { get; private set; } = string.Empty;
    public int OpponentCharacterIndex { get; private set; } = -1;

    public bool BothPlayersReady => InRoom && OpponentPresent && LocalReady && OpponentReady;

    public void Reset()
    {
        InRoom = false;
        IsHost = false;
        OpponentPresent = false;
        LocalReady = false;
        OpponentReady = false;
        LobbyCode = string.Empty;
        OpponentCharacterIndex = -1;
    }

    public string CreateLobby()
    {
        Reset();
        InRoom = true;
        IsHost = true;
        LobbyCode = GenerateLobbyCode();
        return LobbyCode;
    }

    public string JoinLobby(string rawCode)
    {
        Reset();
        InRoom = true;
        IsHost = false;
        LobbyCode = NormalizeLobbyCode(rawCode);
        return LobbyCode;
    }

    public bool ToggleLocalReady()
    {
        LocalReady = !LocalReady;
        return LocalReady;
    }

    public void SetOpponentJoined(int opponentCharacterIndex)
    {
        OpponentPresent = true;
        OpponentCharacterIndex = opponentCharacterIndex;
    }

    public void SetOpponentReady(bool isReady)
    {
        OpponentReady = OpponentPresent && isReady;
    }

    public string GetStatusText()
    {
        if (!InRoom)
        {
            return NotReadyStatus;
        }

        if (BothPlayersReady)
        {
            return BothPlayersReadyStatus;
        }

        if (!OpponentPresent)
        {
            return LocalReady ? WaitingForPlayerWhileReadyStatus : WaitingForPlayerStatus;
        }

        if (LocalReady && !OpponentReady)
        {
            return WaitingForOpponentReadyStatus;
        }

        if (!LocalReady && OpponentReady)
        {
            return OpponentReadyStatus;
        }

        if (LocalReady)
        {
            return ReadyStatus;
        }

        return OpponentJoinedStatus;
    }

    public int PickRandomDigit(int minInclusive, int maxExclusive)
    {
        return random.Next(minInclusive, maxExclusive);
    }

    public static string NormalizeLobbyCode(string rawCode)
    {
        return string.IsNullOrWhiteSpace(rawCode)
            ? string.Empty
            : rawCode.Trim().ToUpperInvariant();
    }

    private string GenerateLobbyCode()
    {
        return string.Concat(
            PickRandomDigit(0, 10),
            PickRandomDigit(0, 10),
            PickRandomDigit(0, 10),
            PickRandomDigit(0, 10),
            PickRandomDigit(0, 10),
            PickRandomDigit(0, 10));
    }
}
