using UnityEngine;

public class TournamentStateData : MonoBehaviour
{
    public enum TournamentRound
    {
        None,
        SemiFinal,
        Final
    }

    public static TournamentStateData Instance;

    // indices into your portraits/prefabs arrays
    public int tl; // player
    public int bl; // player opponent
    public int tr;
    public int br;

    public int finalLeft;
    public int finalRight;

    public int champion = -1;

    // progress flags
    public bool bracketGenerated = false;   // shuffle finished once
    public bool otherMatchResolved = false; // TR vs BR winner chosen
    public bool playerMatchResolved = false; // TL vs BL finished in gameplay
    public bool finalResolved = false;

    public string otherMatchScore = "";
    public string finalScore = "";

    // who you should fight next
    public int nextOpponentIndex = -1; // set before going to GameScene
    public TournamentRound activeRound = TournamentRound.None;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetTournament()
    {
        tl = -1;
        bl = -1;
        tr = -1;
        br = -1;
        finalLeft = -1;
        finalRight = -1;
        champion = -1;
        bracketGenerated = false;
        otherMatchResolved = false;
        playerMatchResolved = false;
        finalResolved = false;
        otherMatchScore = "";
        finalScore = "";
        nextOpponentIndex = -1;
        activeRound = TournamentRound.None;
    }

    public void PrepareMatch(TournamentRound round, int opponentIndex)
    {
        activeRound = round;
        nextOpponentIndex = opponentIndex;
    }

    public void ClearPendingMatch()
    {
        activeRound = TournamentRound.None;
        nextOpponentIndex = -1;
    }
}
