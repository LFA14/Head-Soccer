using TMPro;
using UnityEngine;

public class MatchTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float matchTime = 90f;

    [Header("Game Over")]
    public GameObject gameOverImage;     // 👈 UI image
    public GameObject[] objectsToStop;   // 👈 ball + players

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

        // ✅ Stop gameplay
        foreach (GameObject obj in objectsToStop)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // ✅ Show Game Over
        if (gameOverImage != null)
            gameOverImage.SetActive(true);
    }
}