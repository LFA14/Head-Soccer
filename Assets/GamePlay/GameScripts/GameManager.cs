using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    [Header("Countdown")]
    public Image countdownImage;
    public Sprite threeSprite;
    public Sprite twoSprite;
    public Sprite oneSprite;
    public GameObject[] objectsToDisableAtStart;

    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    [Header("Objects to Reset")]
    public GameObject ball;
    public GameObject player1;
    public GameObject player2;

    [Header("Spawn Points")]
    public Transform ballSpawnPoint;
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    private bool goalScored = false;

    void Start()
    {
        UpdateScoreUI();
        StartCoroutine(Countdown());
    }

   public void PlayerScored(int playerNumber)
{
    Debug.Log("Point awarded to player: " + playerNumber);

    if (goalScored) return;
    goalScored = true;

    if (playerNumber == 1)
        player1Score++;
    else
        player2Score++;

    UpdateScoreUI();
    StartCoroutine(ResetAfterGoal());
}
    void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = player1Score.ToString();

        if (player2ScoreText != null)
            player2ScoreText.text = player2Score.ToString();
    }

    IEnumerator ResetAfterGoal()
    {
        SetGameplay(false);

        yield return new WaitForSeconds(1f);

        ResetObjects();

        yield return StartCoroutine(Countdown());

        goalScored = false;
    }

    void ResetObjects()
    {
        if (ball != null && ballSpawnPoint != null)
            ball.transform.position = ballSpawnPoint.position;

        if (player1 != null && player1SpawnPoint != null)
            player1.transform.position = player1SpawnPoint.position;

        if (player2 != null && player2SpawnPoint != null)
            player2.transform.position = player2SpawnPoint.position;

        Rigidbody2D ballRb = ball != null ? ball.GetComponent<Rigidbody2D>() : null;
        Rigidbody2D p1Rb = player1 != null ? player1.GetComponent<Rigidbody2D>() : null;
        Rigidbody2D p2Rb = player2 != null ? player2.GetComponent<Rigidbody2D>() : null;

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        if (p1Rb != null)
        {
            p1Rb.linearVelocity = Vector2.zero;
            p1Rb.angularVelocity = 0f;
        }

        if (p2Rb != null)
        {
            p2Rb.linearVelocity = Vector2.zero;
            p2Rb.angularVelocity = 0f;
        }
    }

    IEnumerator Countdown()
    {
        SetGameplay(false);

        countdownImage.gameObject.SetActive(true);

        yield return ShowNumber(threeSprite);
        yield return ShowNumber(twoSprite);
        yield return ShowNumber(oneSprite);

        countdownImage.gameObject.SetActive(false);

        SetGameplay(true);
    }

    IEnumerator ShowNumber(Sprite sprite)
    {
        countdownImage.sprite = sprite;
        yield return null;
        yield return StartCoroutine(PopEffect());
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator PopEffect()
    {
        float duration = 0.25f;
        float t = 0;

        Vector3 start = Vector3.zero;
        Vector3 peak = Vector3.one * 1.3f;
        Vector3 end = Vector3.one;

        countdownImage.transform.localScale = start;

        while (t < duration)
        {
            t += Time.deltaTime;
            countdownImage.transform.localScale = Vector3.Lerp(start, peak, t / duration);
            yield return null;
        }

        t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            countdownImage.transform.localScale = Vector3.Lerp(peak, end, t / duration);
            yield return null;
        }

        countdownImage.transform.localScale = end;
    }

    void SetGameplay(bool state)
    {
        foreach (GameObject obj in objectsToDisableAtStart)
        {
            if (obj != null)
                obj.SetActive(state);
            else
                Debug.LogWarning("Missing object in disable list!");
        }
    }
}