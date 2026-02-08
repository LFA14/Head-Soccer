using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character prefabs (used later for spawning)")]
    public GameObject[] characterPrefabs;

    [Header("Portrait UI Images (inside the frames)")]
    public Image playerPortrait;
    public Image comPortrait;

    [Header("Portrait sprites (what you show in the selector)")]
    public Sprite[] portraitSprites;

    [Header("Indexes")]
    public int playerIndex = 0;
    public int comIndex = 0;

    [Header("Animation")]
    public float popScale = 1.12f;
    public float popDuration = 0.12f;

    public void PlayerUp()  { playerIndex = Prev(playerIndex); ApplyPlayer(true); }
    public void PlayerDown(){ playerIndex = Next(playerIndex); ApplyPlayer(true); }

    public void ComUp()     { comIndex = Prev(comIndex); ApplyCom(true); }
    public void ComDown()   { comIndex = Next(comIndex); ApplyCom(true); }

    int Next(int i)
    {
        if (portraitSprites.Length == 0) return 0;
        return (i + 1) % portraitSprites.Length;
    }

    int Prev(int i)
    {
        if (portraitSprites.Length == 0) return 0;
        return (i - 1 + portraitSprites.Length) % portraitSprites.Length;
    }

    void ApplyPlayer(bool animate)
    {
        if (playerPortrait == null || portraitSprites.Length == 0) return;
        playerPortrait.sprite = portraitSprites[playerIndex];
        var c = playerPortrait.color; c.a = 1f; playerPortrait.color = c; // ensure visible
        if (animate) StartCoroutine(Pop(playerPortrait.rectTransform));
    }

    void ApplyCom(bool animate)
    {
        if (comPortrait == null || portraitSprites.Length == 0) return;
        comPortrait.sprite = portraitSprites[comIndex];
        var c = comPortrait.color; c.a = 1f; comPortrait.color = c; // ensure visible
        if (animate) StartCoroutine(Pop(comPortrait.rectTransform));
    }

    IEnumerator Pop(RectTransform t)
    {
        if (t == null) yield break;

        Vector3 start = Vector3.one;
        Vector3 peak = Vector3.one * popScale;

        float half = popDuration * 0.5f;
        float time = 0f;

        // up
        while (time < half)
        {
            time += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(time / half);
            t.localScale = Vector3.Lerp(start, peak, k);
            yield return null;
        }

        // down
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

    // Call this BEFORE loading GameScene
    public void ConfirmSelection()
    {
        if (SelectionData.Instance == null) return;

        // Save prefabs to spawn later
        if (characterPrefabs != null && characterPrefabs.Length > 0)
        {
            if (playerIndex < characterPrefabs.Length) SelectionData.Instance.playerPrefab = characterPrefabs[playerIndex];
            if (comIndex < characterPrefabs.Length) SelectionData.Instance.comPrefab = characterPrefabs[comIndex];
        }
    }

    // Optional: call once on Start to show initial characters
    void Start()
    {
        ApplyPlayer(false);
        ApplyCom(false);
    }
}
