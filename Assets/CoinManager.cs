using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Starting Values")]
    public int startingCoins = 100;

    [Header("Testing")]
    public bool resetCoinsOnPlay = false;
    public bool resetAllSavesOnPlay = false;

    public int Coins { get; private set; }

    private const string CoinsKey = "Coins";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (resetAllSavesOnPlay)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }
            else if (resetCoinsOnPlay)
            {
                PlayerPrefs.DeleteKey(CoinsKey);
                PlayerPrefs.Save();
            }

            if (!PlayerPrefs.HasKey(CoinsKey))
            {
                Coins = startingCoins;
                PlayerPrefs.SetInt(CoinsKey, Coins);
                PlayerPrefs.Save();
            }
            else
            {
                Coins = PlayerPrefs.GetInt(CoinsKey);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        Coins += amount;

        if (Coins < 0)
            Coins = 0;

        SaveCoins();
    }

    public void SetCoins(int amount)
    {
        Coins = Mathf.Max(0, amount);
        SaveCoins();
    }

    public void ResetCoins()
    {
        Coins = startingCoins;
        SaveCoins();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
    }
}