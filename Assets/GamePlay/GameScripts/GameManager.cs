using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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
    public Sprite goalSprite;
    public float goalDisplayDuration = 3f;
    public Vector2 goalImageSize = new Vector2(700f, 500f);
    public AudioClip goalSfx;
    [Range(0f, 1f)] public float goalSfxVolume = 1f;
    public AudioClip crowdSfx;
    [Range(0f, 1f)] public float crowdSfxVolume = 0.45f;
    [Range(0f, 1f)] public float gameplayMusicVolumeMultiplier = 0.12f;
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

    [Header("Quick Match Rewards")]
    public int quickMatchWinReward = 10;
    public int quickMatchLoseReward = 5;

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
    private bool resultProcessed = false;
    private bool isSuddenDeath = false;

    private float currentTime;
    private float suddenDeathTime = 0f;
    private TournamentMatchResultHandler tournamentMatchResultHandler;
    private Vector2 defaultCountdownSize = new Vector2(300f, 300f);
    private AudioSource goalAudioSource;
    private AudioSource crowdAudioSource;
    private readonly List<PowerBarState> savedPowerBarStates = new List<PowerBarState>();

    private struct PowerBarState
    {
        public PowerFill bar;
        public float progress;
    }

    void Awake()
    {
        EnsureGoalAudioSource();
        EnsureCrowdAudioSource();
    }

    void Start()
    {
        currentTime = matchTime;
        UpdateScoreUI();
        UpdateTimerUI();
        tournamentMatchResultHandler = FindObjectOfType<TournamentMatchResultHandler>();
        UpdateMusicDucking(true);

        if (gameOverImage != null)
            gameOverImage.SetActive(false);

        if (endGameButton != null)
            endGameButton.SetActive(false);

        if (GameModeManager.IsOnlineMatch && PhotonNetwork.InRoom)
        {
            StartCoroutine(InitializeOnlineMatchWhenPlayersArePresent());
            return;
        }

        InitializeSpawnedMatch();
    }

    IEnumerator InitializeOnlineMatchWhenPlayersArePresent()
    {
        float waitDeadline = 10f;

        while (waitDeadline > 0f)
        {
            FindPlayers();

            if (leftPlayerHead != null && rightPlayerHead != null)
                break;

            waitDeadline -= Time.deltaTime;
            yield return null;
        }

        InitializeSpawnedMatch();
    }

    void InitializeSpawnedMatch()
    {
        FindPlayers();
        CachePlayerReferences();
        PowerFill.ResetAllBarsInScene();
        StartCoroutine(Countdown());
    }

    void Update()
    {
        if (!matchTimerRunning || matchEnded)
            return;

        if (isSuddenDeath)
        {
            suddenDeathTime += Time.deltaTime;
            UpdateTimerUI();
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;

        UpdateTimerUI();

        if (currentTime <= 0f)
            HandleEndOfNormalTime();
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

        if (isSuddenDeath)
        {
            Debug.Log("Sudden Death goal scored by side " + playerNumber + ". Ending match immediately.");
            goalScored = false;
            EndMatch();
            return;
        }

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
        {
            if (isSuddenDeath)
                timerText.text = "+" + Mathf.CeilToInt(suddenDeathTime).ToString("00");
            else
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    void HandleEndOfNormalTime()
    {
        if (player1Score == player2Score)
        {
            EnterSuddenDeath();
            return;
        }

        EndMatch();
    }

    void EnterSuddenDeath()
    {
        if (isSuddenDeath)
            return;

        isSuddenDeath = true;
        suddenDeathTime = 0f;
        UpdateTimerUI();
        Debug.Log("Match ended in a draw. Entering Sudden Death.");
    }

    IEnumerator ResetAfterGoal()
    {
        SavePowerBarStates();
        SetGameplay(false);
        matchTimerRunning = false;
        ResetAllSpecialStates();

        yield return StartCoroutine(ShowGoalCelebration());

        FindPlayers();
        CachePlayerReferences();
        ResetObjects();

        yield return StartCoroutine(Countdown());
        RestorePowerBarStates();

        goalScored = false;
    }

    IEnumerator ShowGoalCelebration()
    {
        if (countdownImage == null || goalSprite == null)
        {
            yield return new WaitForSeconds(goalDisplayDuration);
            yield break;
        }

        RectTransform countdownRect = countdownImage.rectTransform;
        Vector2 originalSize = countdownRect != null ? countdownRect.sizeDelta : defaultCountdownSize;
        Sprite originalSprite = countdownImage.sprite;
        bool originalPreserveAspect = countdownImage.preserveAspect;
        Color originalColor = countdownImage.color;
        Vector3 originalScale = countdownImage.transform.localScale;
        Quaternion originalRotation = countdownImage.transform.localRotation;

        countdownImage.gameObject.SetActive(true);
        countdownImage.sprite = goalSprite;
        countdownImage.preserveAspect = true;
        countdownImage.color = new Color(1f, 1f, 1f, 0f);
        countdownImage.transform.localScale = Vector3.one * 0.25f;
        countdownImage.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
        PlayGoalSound();

        if (countdownRect != null)
            countdownRect.sizeDelta = goalImageSize;

        float entranceDuration = 0.4f;
        float settleDuration = 0.2f;
        float exitDuration = 0.45f;
        float holdDuration = Mathf.Max(0f, goalDisplayDuration - entranceDuration - settleDuration - exitDuration);
        float t = 0f;
        Vector3 introStartScale = Vector3.one * 0.25f;
        Vector3 introPeakScale = Vector3.one * 1.22f;
        Vector3 finalScale = Vector3.one;

        while (t < entranceDuration)
        {
            t += Time.deltaTime;
            float progress = EaseOutCubic(Mathf.Clamp01(t / entranceDuration));

            countdownImage.color = new Color(1f, 1f, 1f, progress);
            countdownImage.transform.localScale = Vector3.LerpUnclamped(introStartScale, introPeakScale, progress);
            countdownImage.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-10f, 3f, progress));
            yield return null;
        }

        t = 0f;

        while (t < settleDuration)
        {
            t += Time.deltaTime;
            float progress = EaseOutBack(Mathf.Clamp01(t / settleDuration));

            countdownImage.color = Color.white;
            countdownImage.transform.localScale = Vector3.LerpUnclamped(introPeakScale, finalScale, progress);
            countdownImage.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(3f, 0f, progress));
            yield return null;
        }

        float elapsedHold = 0f;

        while (elapsedHold < holdDuration)
        {
            elapsedHold += Time.deltaTime;

            float pulse = 1f + Mathf.Sin(elapsedHold * 12f) * 0.03f;
            float shake = Mathf.Sin(elapsedHold * 42f) * 1.6f;

            countdownImage.color = Color.white;
            countdownImage.transform.localScale = finalScale * pulse;
            countdownImage.transform.localRotation = Quaternion.Euler(0f, 0f, shake);
            yield return null;
        }

        t = 0f;
        Vector3 exitScale = Vector3.one * 1.35f;

        while (t < exitDuration)
        {
            t += Time.deltaTime;
            float progress = EaseInCubic(Mathf.Clamp01(t / exitDuration));

            countdownImage.color = new Color(1f, 1f, 1f, 1f - progress);
            countdownImage.transform.localScale = Vector3.LerpUnclamped(finalScale, exitScale, progress);
            countdownImage.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -6f, progress));
            yield return null;
        }

        countdownImage.sprite = originalSprite;
        countdownImage.preserveAspect = originalPreserveAspect;
        countdownImage.color = originalColor;
        countdownImage.transform.localScale = originalScale;
        countdownImage.transform.localRotation = originalRotation;

        if (countdownRect != null)
            countdownRect.sizeDelta = originalSize;

        countdownImage.gameObject.SetActive(false);
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

        countdownImage.rectTransform.sizeDelta = defaultCountdownSize;
        countdownImage.sprite = sprite;
        countdownImage.preserveAspect = false;
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

    float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    float EaseInCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * t;
    }

    float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float overshoot = 1.70158f;
        float shifted = t - 1f;
        return 1f + (overshoot + 1f) * shifted * shifted * shifted + overshoot * shifted * shifted;
    }

    void EnsureGoalAudioSource()
    {
        if (goalAudioSource != null)
            return;

        goalAudioSource = GetComponent<AudioSource>();

        if (goalAudioSource == null)
            goalAudioSource = gameObject.AddComponent<AudioSource>();

        goalAudioSource.playOnAwake = false;
        goalAudioSource.loop = false;
        goalAudioSource.spatialBlend = 0f;
    }

    void EnsureCrowdAudioSource()
    {
        if (crowdAudioSource != null)
            return;

        crowdAudioSource = gameObject.AddComponent<AudioSource>();
        crowdAudioSource.playOnAwake = false;
        crowdAudioSource.loop = true;
        crowdAudioSource.spatialBlend = 0f;
        crowdAudioSource.volume = crowdSfxVolume;
    }

    void PlayGoalSound()
    {
        if (goalSfx == null)
            return;

        EnsureGoalAudioSource();

        if (goalAudioSource == null)
            return;

        goalAudioSource.PlayOneShot(goalSfx, goalSfxVolume);
    }

    void UpdateCrowdSound(bool shouldPlay)
    {
        if (crowdSfx == null)
            return;

        EnsureCrowdAudioSource();

        if (crowdAudioSource == null)
            return;

        crowdAudioSource.volume = crowdSfxVolume;

        if (shouldPlay)
        {
            if (crowdAudioSource.clip != crowdSfx)
                crowdAudioSource.clip = crowdSfx;

            if (!crowdAudioSource.isPlaying)
                crowdAudioSource.Play();
        }
        else if (crowdAudioSource.isPlaying)
        {
            crowdAudioSource.Pause();
        }
    }

    void SetGameplay(bool state)
    {
        gameplayEnabled = state;
        UpdateCrowdSound(state && !matchEnded);

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
        UpdateCrowdSound(false);
        UpdateMusicDucking(false);
        PowerFill.ResetAllBarsInScene();
        ResetAllSpecialStates();
        FinalizeMatchResultIfNeeded();

        if (timerText != null)
        {
            if (isSuddenDeath)
                timerText.text = "+" + Mathf.CeilToInt(suddenDeathTime).ToString("00");
            else
                timerText.text = "0";
        }

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

    public void FinalizeMatchResultIfNeeded()
    {
        if (resultProcessed)
            return;

        if (MatchContext.Instance == null)
        {
            Debug.Log("No MatchContext found. Skipping tournament result processing.");
            resultProcessed = true;
            return;
        }

        Debug.Log("Match ended with score " + player1Score + " - " + player2Score +
                  " in mode " + MatchContext.Instance.currentMode);

        int playerScore;
        int opponentScore;
        GetPlayerAndOpponentScores(out playerScore, out opponentScore);
        bool playerWon = DidPlayerWinMatch(playerScore, opponentScore);

        if (MatchContext.Instance.currentMode == MatchContext.MatchMode.QuickMatch)
        {
            int reward = playerWon ? quickMatchWinReward : quickMatchLoseReward;

            if (CoinManager.Instance != null)
                CoinManager.Instance.AddCoins(reward);

            TournamentResultData.GetOrCreate().SetResult(
                playerWon,
                false,
                false,
                false,
                reward
            );

            resultProcessed = true;
            return;
        }

        if (MatchContext.Instance.currentMode != MatchContext.MatchMode.Tournament)
        {
            resultProcessed = true;
            return;
        }

        if (tournamentMatchResultHandler == null)
            tournamentMatchResultHandler = FindObjectOfType<TournamentMatchResultHandler>();

        if (tournamentMatchResultHandler == null)
        {
            Debug.LogError("Tournament match ended but no TournamentMatchResultHandler was found in GameScene.");
            return;
        }

        if (tournamentMatchResultHandler.FinishTournamentMatch(playerWon, playerScore, opponentScore))
            resultProcessed = true;
    }

    bool DidPlayerWinMatch(int playerScore, int opponentScore)
    {
        Debug.Log("Resolved player score = " + playerScore + ", opponent score = " + opponentScore);

        if (playerScore == opponentScore)
        {
            Debug.LogWarning("Match ended in a draw. Treating it as a loss for knockout progression.");
            return false;
        }

        return playerScore > opponentScore;
    }

    void GetPlayerAndOpponentScores(out int playerScore, out int opponentScore)
    {
        bool playerIsLeftSide;

        if (MatchContext.Instance != null)
            playerIsLeftSide = !MatchContext.Instance.playerIsOnRightSide;
        else
            playerIsLeftSide = IsPlayerOnLeftSide();

        if (playerIsLeftSide)
        {
            playerScore = player1Score;
            opponentScore = player2Score;
        }
        else
        {
            playerScore = player2Score;
            opponentScore = player1Score;
        }
    }

    bool IsPlayerOnLeftSide()
    {
        if (leftPlayerMovement != null)
            return leftPlayerMovement.isPlayer;

        if (rightPlayerMovement != null)
            return !rightPlayerMovement.isPlayer;

        if (leftKick != null)
            return leftKick.isPlayer;

        if (rightKick != null)
            return !rightKick.isPlayer;

        Debug.LogWarning("Could not detect player side reliably. Falling back to left side.");
        return true;
    }

    public void LoadNextScene()
    {
        Debug.Log("BUTTON CLICKED");
        SceneManager.LoadScene(1);
    }

    void ResetAllSpecialStates()
    {
        CharacterSpecialController[] specials = FindObjectsOfType<CharacterSpecialController>(true);

        for (int i = 0; i < specials.Length; i++)
        {
            if (specials[i] != null)
                specials[i].ResetSpecialState();
        }
    }

    void SavePowerBarStates()
    {
        savedPowerBarStates.Clear();

        PowerFill[] bars = FindObjectsOfType<PowerFill>(true);
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            savedPowerBarStates.Add(new PowerBarState
            {
                bar = bars[i],
                progress = bars[i].NormalizedProgress
            });
        }
    }

    void RestorePowerBarStates()
    {
        for (int i = 0; i < savedPowerBarStates.Count; i++)
        {
            PowerBarState state = savedPowerBarStates[i];
            if (state.bar == null)
                continue;

            state.bar.SetProgressNormalized(state.progress);
        }

        savedPowerBarStates.Clear();
    }

    void UpdateMusicDucking(bool gameplayActive)
    {
        if (MenuMusic.Instance == null)
            return;

        if (gameplayActive && !matchEnded)
        {
            MenuMusic.Instance.SetGameplayMuted(true);
            MenuMusic.Instance.SetVolumeMultiplier(0f);
        }
        else
        {
            MenuMusic.Instance.SetGameplayMuted(false);
            MenuMusic.Instance.RestoreOriginalVolume();
        }
    }

    void OnDisable()
    {
        UpdateMusicDucking(false);
    }
}
