using UnityEngine;

public class TournamentMatchResultHandler : MonoBehaviour
{
    [Header("Rewards")]
    public int winReward = 10;
    public int loseReward = 5;

    public void FinishTournamentMatch(bool playerWon)
    {
        if (TournamentStateData.Instance == null)
        {
            Debug.LogError("TournamentStateData.Instance is missing.");
            return;
        }

        if (TournamentResultData.Instance == null)
        {
            Debug.LogError("TournamentResultData.Instance is missing.");
            return;
        }

        var st = TournamentStateData.Instance;

        bool isSemiFinal = !st.playerMatchResolved;
        bool isFinal = st.playerMatchResolved && !st.finalResolved;

        int reward = playerWon ? winReward : loseReward;

        // Add the real coins to the player
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(reward);

        if (isSemiFinal)
        {
            st.playerMatchResolved = true;

            if (playerWon)
            {
                // Player qualifies to final left slot
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
                // Opponent takes final left slot
                st.finalLeft = st.bl;

                TournamentResultData.Instance.SetResult(
                    false,  // playerWon
                    false,  // qualified
                    false,  // wonTournament
                    false,  // wasFinalMatch
                    reward
                );
            }
        }
        else if (isFinal)
        {
            st.finalResolved = true;

            if (playerWon)
            {
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
        }

        Debug.Log("Tournament updated. playerWon = " + playerWon +
                  ", finalLeft = " + st.finalLeft +
                  ", finalRight = " + st.finalRight +
                  ", champion = " + st.champion);
    }
}