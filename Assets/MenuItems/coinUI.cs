using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public Image coinImage;
    public Sprite[] coinFrames;
    public float frameRate = 0.08f;

    public TextMeshProUGUI coinsText;

    private float timer;
    private int frameIndex;

    void Update()
    {
        AnimateCoin();
        UpdateCoinsText();
    }

    void AnimateCoin()
    {
        if (coinImage == null || coinFrames == null || coinFrames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0f;
            frameIndex = (frameIndex + 1) % coinFrames.Length;
            coinImage.sprite = coinFrames[frameIndex];
        }
    }

    void UpdateCoinsText()
    {
        if (coinsText == null || CoinManager.Instance == null)
            return;

        coinsText.text = CoinManager.Instance.Coins.ToString();
    }
}