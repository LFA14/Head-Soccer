using UnityEngine;

public class MatchRewardManager : MonoBehaviour
{
    public void RewardWin()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(10);
    }

    public void RewardLoss()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(5);
    }
}