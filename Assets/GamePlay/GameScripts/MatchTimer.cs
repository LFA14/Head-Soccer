using TMPro;
using UnityEngine;

public class MatchTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float matchTime = 90f;

    private bool isRunning = true;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        if (matchTime > 0)
        {
            matchTime -= Time.deltaTime;
            matchTime = Mathf.Max(matchTime, 0);
            UpdateUI();
        }
        else
        {
            EndMatch();
        }
    }

    void UpdateUI()
    {
        timerText.text = Mathf.CeilToInt(matchTime).ToString();
    }

    void EndMatch()
    {
        isRunning = false;
        timerText.text = "0";

        // Later you can add:
        // Time.timeScale = 0;
        // Show win screen
        // Disable controls
    }
}