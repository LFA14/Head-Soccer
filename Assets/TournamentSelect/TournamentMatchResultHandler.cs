using UnityEngine;

public class TournamentMatchResultHandler : MonoBehaviour
{
    [Header("Rewards")]
    public int winReward = 10;
    public int loseReward = 5;
    public int finalWinReward = 20;

    public bool FinishTournamentMatch(bool playerWon, int playerScore, int opponentScore)
    {
        if (TournamentStateData.Instance == null)
        {
            Debug.LogError("TournamentStateData.Instance is missing.");
            return false;
        }

        if (TournamentResultData.Instance == null)
        {
            Debug.LogError("TournamentResultData.Instance is missing.");
            return false;
        }

        var st = TournamentStateData.Instance;
        if (st.activeRound == TournamentStateData.TournamentRound.None)
        {
            Debug.LogWarning("FinishTournamentMatch called with no active tournament round. Ignoring duplicate or invalid call.");
            return false;
        }

        TournamentStateData.TournamentRound resolvedRound = st.activeRound;
        int reward = playerWon ? winReward : loseReward;
        bool handled = false;

        if (st.activeRound == TournamentStateData.TournamentRound.SemiFinal)
        {
            if (st.playerMatchResolved)
            {
                Debug.LogWarning("Semi-final result was already resolved. Ignoring duplicate result call.");
                return false;
            }

            st.playerMatchResolved = true;
            st.playerMatchScore = playerScore + " - " + opponentScore;

            if (playerWon)
            {
                st.finalLeft = st.tl;

                TournamentResultData.Instance.SetResult(
                    true,   // playerWon
                    true,   // qualified
                    false,  // wonTournament
                    false,  // wasFinalMatch
                    reward
                );
            }
            else
            {
                st.finalLeft = st.bl;

                TournamentResultData.Instance.SetResult(
                    false,  // playerWon
                    false,  // qualified
                    false,  // wonTournament
                    false,  // wasFinalMatch
                    reward
                );
            }

            handled = true;
        }
        else if (st.activeRound == TournamentStateData.TournamentRound.Final)
        {
            if (st.finalResolved)
            {
                Debug.LogWarning("Final result was already resolved. Ignoring duplicate result call.");
                return false;
            }

            st.finalResolved = true;
            st.finalScore = playerScore + " - " + opponentScore;

            if (playerWon)
            {
                reward = finalWinReward > 0 ? finalWinReward : 20;
                st.champion = st.finalLeft;

                TournamentResultData.Instance.SetResult(
                    true,   // playerWon
                    true,   // qualified
                    true,   // wonTournament
                    true,   // wasFinalMatch
                    reward
                );
            }
            else
            {
                st.champion = st.finalRight;

                TournamentResultData.Instance.SetResult(
                    false,  // playerWon
                    false,  // qualified
                    false,  // wonTournament
                    true,   // wasFinalMatch
                    reward
                );
            }

            handled = true;
        }

        if (!handled)
        {
            Debug.LogWarning("Tournament result was not handled because the current state is inconsistent.");
            return false;
        }

        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(reward);
        else
            Debug.LogWarning("CoinManager.Instance is missing, so tournament reward could not be added.");

        st.ClearPendingMatch();

        Debug.Log("Tournament updated. playerWon = " + playerWon +
                  ", round = " + resolvedRound +
                  ", finalLeft = " + st.finalLeft +
                  ", finalRight = " + st.finalRight +
                  ", champion = " + st.champion +
                  ", reward = " + reward);

        return true;
    }

    public bool FinishTournamentMatch(bool playerWon)
    {
        return FinishTournamentMatch(playerWon, 0, 0);
    }
}
