using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI p1Text;
    public TextMeshProUGUI p2Text;

    private int p1Score = 0;
    private int p2Score = 0;

    void Start()
    {
        UpdateUI();
    }

    public void Player1Scored()
    {
        p1Score++;
        UpdateUI();
    }

    public void Player2Scored()
    {
        p2Score++;
        UpdateUI();
    }

    void UpdateUI()
    {
        p1Text.text = p1Score.ToString();
        p2Text.text = p2Score.ToString();
    }
}