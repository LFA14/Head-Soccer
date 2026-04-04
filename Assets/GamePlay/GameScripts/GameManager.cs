using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Match Timer")]
    public float matchTime = 10f;
    public TextMeshProUGUI timerText;

    [Header("Game Over")]
    public GameObject gameOverImage;
    public GameObject endGameButton;
    public string sceneToLoad = "MenuScene";

    [Header("Power Bars")]
    public GameObject powerBar1;
    public GameObject powerBar2;
    public GameObject powerBar3;
    public GameObject powerBar4;

    [Header("Objects")]
    public GameObject ball;

    [Header("Spawn Points")]
    public Transform ballSpawnPoint;
    public Transform leftPlayerSpawnPoint;
    public Transform rightPlayerSpawnPoint;

    private Transform leftPlayerHead;
    private Transform rightPlayerHead;

    private Transform leftPlayerRoot;
    private Transform rightPlayerRoot;

    private Rigidbody2D ballRb;
    private Rigidbody2D leftPlayerRb;
    private Rigidbody2D rightPlayerRb;

    private PlayerMovement leftPlayerMovement;
    private PlayerMovement rightPlayerMovement;

    private KickController leftKick;
    private KickController rightKick;

    private SimpleAI leftAI;
    private SimpleAI rightAI;

    private bool goalScored = false;
    private bool gameplayEnabled = false;
    private bool matchTimerRunning = false;
    private bool matchEnded = false;

    private float currentTime;

    void Start()
    {
        currentTime = matchTime;
        UpdateScoreUI();
        UpdateTimerUI();

        if (gameOverImage != null)
            gameOverImage.SetActive(false);

        if (endGameButton != null)
            endGameButton.SetActive(false);

        FindPlayers();
        CachePlayerReferences();
        StartCoroutine(Countdown());
    }

    void Update()
    {
        if (!matchTimerRunning || matchEnded)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;

        UpdateTimerUI();

        if (currentTime <= 0f)
        {
            EndMatch();
        }
    }

    public void PlayerScored(int playerNumber)
    {
        if (goalScored || matchEnded)
            return;

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

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    IEnumerator ResetAfterGoal()
    {
        SetGameplay(false);
        matchTimerRunning = false;

        yield return new WaitForSeconds(1f);

        FindPlayers();
        CachePlayerReferences();
        ResetObjects();

        yield return StartCoroutine(Countdown());

        goalScored = false;
    }

    void FindPlayers()
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

            if (ball != null && rb.gameObject == ball)
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

        leftPlayerHead = foundLeftHead;
        rightPlayerHead = foundRightHead;

        leftPlayerRoot = leftPlayerHead != null ? leftPlayerHead.root : null;
        rightPlayerRoot = rightPlayerHead != null ? rightPlayerHead.root : null;
    }

    void CachePlayerReferences()
    {
        ballRb = ball != null ? ball.GetComponent<Rigidbody2D>() : null;

        leftPlayerRb = leftPlayerHead != null ? leftPlayerHead.GetComponent<Rigidbody2D>() : null;
        rightPlayerRb = rightPlayerHead != null ? rightPlayerHead.GetComponent<Rigidbody2D>() : null;

        leftPlayerMovement = leftPlayerHead != null ? leftPlayerHead.GetComponentInParent<PlayerMovement>() : null;
        rightPlayerMovement = rightPlayerHead != null ? rightPlayerHead.GetComponentInParent<PlayerMovement>() : null;

        leftKick = leftPlayerHead != null ? leftPlayerHead.GetComponentInParent<KickController>() : null;
        rightKick = rightPlayerHead != null ? rightPlayerHead.GetComponentInParent<KickController>() : null;

        leftAI = leftPlayerHead != null ? leftPlayerHead.GetComponentInParent<SimpleAI>() : null;
        rightAI = rightPlayerHead != null ? rightPlayerHead.GetComponentInParent<SimpleAI>() : null;
    }

    void ResetObjects()
    {
        if (ball != null && ballSpawnPoint != null)
        {
            ball.transform.position = ballSpawnPoint.position;
            ball.transform.rotation = ballSpawnPoint.rotation;
        }

        ResetPlayer(leftPlayerRoot, leftPlayerHead, leftPlayerRb, leftPlayerSpawnPoint);
        ResetPlayer(rightPlayerRoot, rightPlayerHead, rightPlayerRb, rightPlayerSpawnPoint);

        StopRigidBody(ballRb);
    }

    void ResetPlayer(Transform playerRoot, Transform playerHead, Rigidbody2D playerRb, Transform spawnPoint)
    {
        if (spawnPoint == null)
            return;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerRoot != null)
        {
            playerRoot.position = spawnPoint.position;
            playerRoot.rotation = spawnPoint.rotation;
        }

        if (playerHead != null)
        {
            playerHead.position = spawnPoint.position;
            playerHead.rotation = spawnPoint.rotation;
        }
    }

    void StopRigidBody(Rigidbody2D rb)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    IEnumerator Countdown()
    {
        SetGameplay(false);

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(true);

        yield return ShowNumber(threeSprite);
        yield return ShowNumber(twoSprite);
        yield return ShowNumber(oneSprite);

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(false);

        StopRigidBody(ballRb);
        StopRigidBody(leftPlayerRb);
        StopRigidBody(rightPlayerRb);

        SetGameplay(true);

        if (!matchEnded)
            matchTimerRunning = true;
    }

    IEnumerator ShowNumber(Sprite sprite)
    {
        if (countdownImage == null)
            yield break;

        countdownImage.sprite = sprite;
        yield return null;
        yield return StartCoroutine(PopEffect());
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator PopEffect()
    {
        if (countdownImage == null)
            yield break;

        float duration = 0.25f;
        float t = 0f;

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

        t = 0f;

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
        gameplayEnabled = state;

        if (!state)
        {
            StopRigidBody(ballRb);
            StopRigidBody(leftPlayerRb);
            StopRigidBody(rightPlayerRb);
        }

        if (leftPlayerMovement != null)
            leftPlayerMovement.enabled = state;

        if (rightPlayerMovement != null)
            rightPlayerMovement.enabled = state;

        if (leftKick != null)
            leftKick.enabled = state;

        if (rightKick != null)
            rightKick.enabled = state;

        if (leftAI != null)
            leftAI.enabled = state;

        if (rightAI != null)
            rightAI.enabled = state;

        if (objectsToDisableAtStart != null)
        {
            foreach (GameObject obj in objectsToDisableAtStart)
            {
                if (obj != null)
                    obj.SetActive(state);
            }
        }
    }

    void EndMatch()
    {
        matchEnded = true;
        matchTimerRunning = false;

        SetGameplay(false);

        if (timerText != null)
            timerText.text = "0";

        if (leftPlayerRoot != null)
            leftPlayerRoot.gameObject.SetActive(false);

        if (rightPlayerRoot != null)
            rightPlayerRoot.gameObject.SetActive(false);

        if (ball != null)
            ball.SetActive(false);

        if (powerBar1 != null)
            powerBar1.SetActive(false);

        if (powerBar2 != null)
            powerBar2.SetActive(false);

        if (powerBar3 != null)
            powerBar3.SetActive(false);

        if (powerBar4 != null)
            powerBar4.SetActive(false);

        if (gameOverImage != null)
            gameOverImage.SetActive(true);

        if (endGameButton != null)
            endGameButton.SetActive(true);
    }

   public void LoadNextScene()
{
    Debug.Log("BUTTON CLICKED");
    SceneManager.LoadScene(1);
}
}