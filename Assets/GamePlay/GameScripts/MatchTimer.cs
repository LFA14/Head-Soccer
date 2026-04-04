using TMPro;
using UnityEngine;

public class MatchTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverImage;
    public GameObject powerBar; // 👈 ADD THIS

    [Header("Timer")]
    public float matchTime = 90f;

    [Header("Ball")]
    public GameObject ball;
    public string ballObjectName = "Ball";

    private bool isRunning = true;

    void Start()
    {
        UpdateUI();

        if (gameOverImage != null)
            gameOverImage.SetActive(false);
    }

    void Update()
    {
        if (!isRunning)
            return;

        if (matchTime > 0f)
        {
            matchTime -= Time.deltaTime;
            matchTime = Mathf.Max(matchTime, 0f);
            UpdateUI();
        }
        else
        {
            EndMatch();
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(matchTime).ToString();
    }

    void EndMatch()
    {
        isRunning = false;

        if (timerText != null)
            timerText.text = "0";

        // 👇 HIDE PLAYERS
        HidePlayers();

        // 👇 HIDE BALL
        HideBall();

        // 👇 HIDE POWER BAR INSTANTLY
        if (powerBar != null)
            powerBar.SetActive(false);

        // 👇 SHOW GAME OVER
        if (gameOverImage != null)
            gameOverImage.SetActive(true);
    }

    void HidePlayers()
    {
        Rigidbody2D[] allBodies = FindObjectsOfType<Rigidbody2D>();

        Transform foundLeftHead = null;
        Transform foundRightHead = null;

        float mostLeftX = float.MaxValue;
        float mostRightX = float.MinValue;

        foreach (Rigidbody2D rb in allBodies)
        {
            if (rb == null)
                continue;

            if (rb.transform.name != "Head")
                continue;

            float x = rb.transform.position.x;

            if (x < mostLeftX)
            {
                mostLeftX = x;
                foundLeftHead = rb.transform;
            }

            if (x > mostRightX)
            {
                mostRightX = x;
                foundRightHead = rb.transform;
            }
        }

        if (foundLeftHead != null && foundLeftHead.root != null)
            foundLeftHead.root.gameObject.SetActive(false);

        if (foundRightHead != null && foundRightHead.root != null)
            foundRightHead.root.gameObject.SetActive(false);
    }

    void HideBall()
    {
        GameObject ballToHide = ball;

        if (ballToHide == null && !string.IsNullOrEmpty(ballObjectName))
            ballToHide = GameObject.Find(ballObjectName);

        if (ballToHide != null)
            ballToHide.SetActive(false);
    }
}