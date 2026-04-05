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

    public static TournamentResultData GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject dataObject = new GameObject("TournamentResultData");
        Instance = dataObject.AddComponent<TournamentResultData>();
        DontDestroyOnLoad(dataObject);
        return Instance;
    }

    public void SetResult(bool won, bool didQualify, bool tournamentWon, bool finalMatch, int reward)
    {
        playerWon = won;
        qualified = didQualify;
        wonTournament = tournamentWon;
        wasFinalMatch = finalMatch;
        rewardCoins = reward;
    }

    public void ClearResult()
    {
        playerWon = false;
        qualified = false;
        wonTournament = false;
        wasFinalMatch = false;
        rewardCoins = 0;
    }
}
