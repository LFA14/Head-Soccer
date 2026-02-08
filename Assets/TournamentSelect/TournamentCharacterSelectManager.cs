using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentCharacterSelectManager : MonoBehaviour
{
    [Header("Character prefabs (spawn later)")]
    public GameObject[] characterPrefabs;   // size = 4

    [Header("Portrait UI (inside the frame)")]
    public Image playerPortrait;            // your PlayerPortrait Image

    [Header("Portrait sprites (what you show in selector)")]
    public Sprite[] portraitSprites;        // size = 4

    [Header("Index")]
    public int playerIndex = 0;

    [Header("Animation")]
    public float popScale = 1.12f;
    public float popDuration = 0.12f;

    void Start()
    {
        ApplyPlayer(false);
    }

    public void PlayerUp()
    {
        playerIndex = Prev(playerIndex);
        ApplyPlayer(true);
    }

    public void PlayerDown()
    {
        playerIndex = Next(playerIndex);
        ApplyPlayer(true);
    }

    int Next(int i)
    {
        if (portraitSprites == null || portraitSprites.Length == 0) return 0;
        return (i + 1) % portraitSprites.Length;
    }

    int Prev(int i)
    {
        if (portraitSprites == null || portraitSprites.Length == 0) return 0;
        return (i - 1 + portraitSprites.Length) % portraitSprites.Length;
    }

    void ApplyPlayer(bool animate)
    {
        if (playerPortrait == null) return;
        if (portraitSprites == null || portraitSprites.Length == 0) return;

        playerPortrait.sprite = portraitSprites[playerIndex];

        // make sure it is visible
        var c = playerPortrait.color;
        c.a = 1f;
        playerPortrait.color = c;

        if (animate) StartCoroutine(Pop(playerPortrait.rectTransform));
    }

    IEnumerator Pop(RectTransform t)
    {
        if (t == null) yield break;

        Vector3 start = Vector3.one;
        Vector3 peak = Vector3.one * popScale;

        float half = popDuration * 0.5f;
        float time = 0f;

        while (time < half)
        {
            time += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(time / half);
            t.localScale = Vector3.Lerp(start, peak, k);
            yield return null;
        }

        time = 0f;
        while (time < half)
        {
            time += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(time / half);
            t.localScale = Vector3.Lerp(peak, start, k);
            yield return null;
        }

        t.localScale = start;
    }

    // Hook this to your PLAY/START button
    public void ConfirmAndGoToTournament(string tournamentBracketSceneName)
    {
        if (TournamentSelectionData.Instance != null)
        {
            TournamentSelectionData.Instance.playerIndex = playerIndex;

            if (characterPrefabs != null && characterPrefabs.Length > 0 && playerIndex < characterPrefabs.Length)
                TournamentSelectionData.Instance.playerPrefab = characterPrefabs[playerIndex];
        }

        SceneManager.LoadScene(tournamentBracketSceneName);
    }
}
