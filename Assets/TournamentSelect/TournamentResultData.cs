using UnityEngine;

public class TournamentResultData : MonoBehaviour
{
    public static TournamentResultData Instance;

    public bool playerWon;
    public bool qualified;
    public bool wonTournament;
    public bool wasFinalMatch;
    public int rewardCoins;

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

    public void SetResult(bool won, bool didQualify, bool tournamentWon, bool finalMatch, int reward)
    {
        playerWon = won;
        qualified = didQualify;
        wonTournament = tournamentWon;
        wasFinalMatch = finalMatch;
        rewardCoins = reward;
    }
}