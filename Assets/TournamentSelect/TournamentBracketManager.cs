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

    [Header("Shuffle")]
    public float shuffleDuration = 1.2f;
    public float shuffleInterval = 0.06f;

    [Header("Scene names")]
    public string gameSceneName = "GameScene"; // your gameplay scene

    void Start()
    {
        if (playMatchBtn != null) playMatchBtn.interactable = false;

        // If bracket not generated yet -> generate + shuffle once
        if (TournamentStateData.Instance == null)
        {
            Debug.LogError("TournamentStateData missing in scene.");
            return;
        }

        if (!TournamentStateData.Instance.bracketGenerated)
        {
            StartCoroutine(GenerateAndShuffleBracket());
        }
        else
        {
            // Already have bracket state -> just redraw
            RedrawAll();
            DecideIfPlayEnabled();
        }
    }

    IEnumerator GenerateAndShuffleBracket()
    {
        // Need selected player from previous scene
        if (TournamentSelectionData.Instance == null)
        {
            Debug.LogError("TournamentSelectionData.Instance is null. Did you come from the select scene?");
            yield break;
        }

        int playerIndex = TournamentSelectionData.Instance.playerIndex;

        // Build list of remaining indices
        List<int> pool = new List<int>();
        for (int i = 0; i < portraitSprites.Length; i++)
            if (i != playerIndex) pool.Add(i);

        // pick 3 random distinct opponents
        Shuffle(pool);
        int opp1 = pool[0];
        int opp2 = pool[1];
        int opp3 = pool[2];

        // store initial bracket layout
        var st = TournamentStateData.Instance;
        st.tl = playerIndex;   // YOU always TL
        st.bl = opp1;          // your opponent BL
        st.tr = opp2;
        st.br = opp3;

        st.bracketGenerated = true;
        st.otherMatchResolved = false;
        st.playerMatchResolved = false;
        st.finalResolved = false;
        st.champion = -1;

        // Visual shuffle effect on 3 AI slots (BL/TR/BR), keep TL fixed
        float t = 0f;
        while (t < shuffleDuration)
        {
            t += shuffleInterval;

            // random faces while shuffling
            SetImage(slotTL, st.tl);
            SetImage(slotBL, RandomIndexNot(playerIndex));
            SetImage(slotTR, RandomIndexNot(playerIndex));
            SetImage(slotBR, RandomIndexNot(playerIndex));

            yield return new WaitForSecondsRealtime(shuffleInterval);
        }

        // stop on the real chosen opponents
        RedrawAll();

        // resolve other match (TR vs BR) immediately/randomly for now
        ResolveOtherMatch();

        DecideIfPlayEnabled();
    }

    void ResolveOtherMatch()
    {
        var st = TournamentStateData.Instance;

        // randomly pick winner of TR vs BR
        st.finalRight = (Random.value < 0.5f) ? st.tr : st.br;
        st.otherMatchResolved = true;

        // your final slot left will be unknown until you play, so just clear for now
        st.finalLeft = -1;

        RedrawAll();
    }

    void DecideIfPlayEnabled()
    {
        var st = TournamentStateData.Instance;

        // you can play semi-final as soon as bracket generated
        bool canPlaySemi = st.bracketGenerated && !st.playerMatchResolved;

        // later: if semi done and final not done => allow final match
        bool canPlayFinal = st.playerMatchResolved && st.otherMatchResolved && !st.finalResolved;

        if (playMatchBtn != null)
            playMatchBtn.interactable = canPlaySemi || canPlayFinal;
    }

    public void OnPlayMatchPressed()
    {
        var st = TournamentStateData.Instance;

        // If semi not done -> fight BL
        if (!st.playerMatchResolved)
        {
            st.nextOpponentIndex = st.bl;
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        // If semi done -> fight finalRight
        if (!st.finalResolved)
        {
            st.nextOpponentIndex = st.finalRight;
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

        // Final slots
        SetImage(finalL, st.finalLeft);
        SetImage(finalR, st.finalRight);

        // Champion slot
        SetImage(champion, st.champion);
    }

    void SetImage(Image img, int index)
    {
        if (img == null) return;

        if (index < 0 || portraitSprites == null || index >= portraitSprites.Length)
        {
            // hide if not assigned yet
            var c = img.color; c.a = 0f; img.color = c;
            img.sprite = null;
            return;
        }

        img.sprite = portraitSprites[index];
        var cc = img.color; cc.a = 1f; img.color = cc;
    }

    int RandomIndexNot(int notThis)
    {
        if (portraitSprites.Length <= 1) return 0;
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
}