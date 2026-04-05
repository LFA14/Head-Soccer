using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentBracketManager : MonoBehaviour
{
    [Header("Portrait Images in bracket (TL, TR, BL, BR, FinalL, FinalR, Champ optional)")]
    public Image slotTL;
    public Image slotTR;
    public Image slotBL;
    public Image slotBR;
    public Image finalL;
    public Image finalR;
    public Image champion;

    [Header("Portrait sprites (same order as your characters)")]
    public Sprite[] portraitSprites;

    [Header("Buttons")]
    public Button playMatchBtn;

    [Header("Score UI (for TR vs BR winner)")]
    public BracketScoreUI scoreUI;

    [Header("Shuffle 1 (initial opponents)")]
    public float shuffleDuration = 0.75f;
    public float shuffleInterval = 0.03f;

    [Header("Other-side semi-final reveal (TR vs BR -> Final Right)")]
    public float otherMatchDelay = 0.0f;               // set 0 for instant
    public float otherMatchShuffleDuration = 0.45f;
    public float otherMatchShuffleInterval = 0.03f;

    [Header("Scene names")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        if (playMatchBtn != null) playMatchBtn.interactable = false;

        if (TournamentStateData.Instance == null)
        {
            Debug.LogError("TournamentStateData missing in scene.");
            return;
        }

        if (!TournamentStateData.Instance.bracketGenerated)
            StartCoroutine(GenerateAndShuffleBracket());
        else
        {
            RedrawAll();
            DecideIfPlayEnabled();
        }
    }

    IEnumerator GenerateAndShuffleBracket()
    {
        if (TournamentSelectionData.Instance == null)
        {
            Debug.LogError("TournamentSelectionData.Instance is null. Did you come from the select scene?");
            yield break;
        }

        if (portraitSprites == null || portraitSprites.Length < 4)
        {
            Debug.LogError("portraitSprites must have at least 4 sprites.");
            yield break;
        }

        int playerIndex = Mathf.Clamp(TournamentSelectionData.Instance.playerIndex, 0, portraitSprites.Length - 1);

        // Build pool of remaining indices
        List<int> pool = new List<int>();
        for (int i = 0; i < portraitSprites.Length; i++)
            if (i != playerIndex) pool.Add(i);

        Shuffle(pool);
        int opp1 = pool[0];
        int opp2 = pool[1];
        int opp3 = pool[2];

        var st = TournamentStateData.Instance;
        st.tl = playerIndex;
        st.bl = opp1;
        st.tr = opp2;
        st.br = opp3;

        st.bracketGenerated = true;
        st.otherMatchResolved = false;
        st.playerMatchResolved = false;
        st.finalResolved = false;
        st.champion = -1;

        st.finalLeft = -1;
        st.finalRight = -1;

        // Show TL instantly (no empty frame)
        SetImage(slotTL, st.tl);

        // ---- Shuffle 1 (BL/TR/BR), using unscaled time (smooth) ----
        float elapsed = 0f;
        float tick = 0f;

        while (elapsed < shuffleDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            tick += dt;

            if (tick >= shuffleInterval)
            {
                tick = 0f;

                SetImage(slotTL, st.tl);
                SetImage(slotBL, RandomIndexNot(playerIndex));
                SetImage(slotTR, RandomIndexNot(playerIndex));
                SetImage(slotBR, RandomIndexNot(playerIndex));
            }

            yield return null;
        }

        // Stop on real opponents
        RedrawAll();
        DecideIfPlayEnabled();

        // Let UI breathe for 1 frame (removes "freeze" feeling)
        yield return null;

        // ---- Other-side reveal (starts fast) ----
        if (otherMatchDelay > 0f)
        {
            float d = 0f;
            while (d < otherMatchDelay)
            {
                d += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return StartCoroutine(ResolveOtherMatchWithShuffle());
        DecideIfPlayEnabled();
    }

    IEnumerator ResolveOtherMatchWithShuffle()
    {
        var st = TournamentStateData.Instance;

        // Make sure finalR shows immediately (no blank gap)
        SetImage(finalR, st.tr);

        float elapsed = 0f;
        float tick = 0f;

        while (elapsed < otherMatchShuffleDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            tick += dt;

            if (tick >= otherMatchShuffleInterval)
            {
                tick = 0f;
                int show = (Random.value < 0.5f) ? st.tr : st.br;
                SetImage(finalR, show);
            }

            yield return null;
        }

        // Pick winner for real + show score
        int winner = (Random.value < 0.5f) ? st.tr : st.br;
        int loser = (winner == st.tr) ? st.br : st.tr;

        st.finalRight = winner;
        st.otherMatchResolved = true;

        int wg, lg;
        MakeKnockoutScore(out wg, out lg);

        if (scoreUI != null)
            scoreUI.Show($"{wg} - {lg}");

        RedrawAll();
    }

    void DecideIfPlayEnabled()
    {
        var st = TournamentStateData.Instance;

        bool canPlaySemi = st.bracketGenerated && !st.playerMatchResolved;
        bool canPlayFinal = st.playerMatchResolved && st.otherMatchResolved && !st.finalResolved;

        if (playMatchBtn != null)
            playMatchBtn.interactable = canPlaySemi || canPlayFinal;
    }

    public void OnPlayMatchPressed()
    {
        var st = TournamentStateData.Instance;

        if (!st.playerMatchResolved)
        {
            st.nextOpponentIndex = st.bl;

            if (MatchContext.Instance != null)
                MatchContext.Instance.SetMode(MatchContext.MatchMode.Tournament);

            SceneManager.LoadScene(gameSceneName);
            return;
        }

        if (st.playerMatchResolved && st.otherMatchResolved && !st.finalResolved)
        {
            st.nextOpponentIndex = st.finalRight;

            if (MatchContext.Instance != null)
                MatchContext.Instance.SetMode(MatchContext.MatchMode.Tournament);

            SceneManager.LoadScene(gameSceneName);
            return;
        }
    }
    public void RedrawAll()
    {
        var st = TournamentStateData.Instance;

        SetImage(slotTL, st.tl);
        SetImage(slotBL, st.bl);
        SetImage(slotTR, st.tr);
        SetImage(slotBR, st.br);

        SetImage(finalL, st.finalLeft);
        SetImage(finalR, st.finalRight);

        SetImage(champion, st.champion);
    }

    void SetImage(Image img, int index)
    {
        if (img == null) return;

        if (index < 0 || portraitSprites == null || index >= portraitSprites.Length)
        {
            var c = img.color; c.a = 0f; img.color = c;
            img.sprite = null;
            return;
        }

        img.sprite = portraitSprites[index];
        var cc = img.color; cc.a = 1f; img.color = cc;
    }

    int RandomIndexNot(int notThis)
    {
        if (portraitSprites == null || portraitSprites.Length <= 1) return 0;

        int r = Random.Range(0, portraitSprites.Length - 1);
        if (r >= notThis) r++;
        return r;
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ----- Score generation (realistic, no ties) -----

    int SampleGoalsBiased()
    {
        int[] goals = { 0, 1, 2, 3, 4, 5 };
        int[] w = { 22, 32, 24, 14, 6, 2 }; // mostly 0-3
        int r = Random.Range(0, 100);
        int sum = 0;
        for (int i = 0; i < w.Length; i++)
        {
            sum += w[i];
            if (r < sum) return goals[i];
        }
        return 1;
    }

    void MakeKnockoutScore(out int winnerGoals, out int loserGoals)
    {
        while (true)
        {
            int a = SampleGoalsBiased();
            int b = SampleGoalsBiased();
            if (a == b) continue;

            winnerGoals = Mathf.Max(a, b);
            loserGoals = Mathf.Min(a, b);
            return;
        }
    }
}