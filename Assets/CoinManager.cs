using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int Coins { get; private set; }

    private const string CoinsKey = "Coins";

   void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey(CoinsKey))
        {
            Coins = 100; // starting coins
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
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
    }

    public void SetCoins(int amount)
    {
        Coins = amount;
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
    }
}