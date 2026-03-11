using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character prefabs (used later for spawning)")]
    public GameObject[] characterPrefabs;

    [Header("Portrait UI Images")]
    public Image playerPortrait;
    public Image comPortrait;

    [Header("Portrait sprites")]
    public Sprite[] portraitSprites;

    [Header("Indexes")]
    public int playerIndex = 0;
    public int comIndex = 0;

    [Header("Animation")]
    public float popScale = 1.12f;
    public float popDuration = 0.12f;

    [Header("Scene")]
    public string gameSceneName = "GameScene";

    public void PlayerUp() { playerIndex = Prev(playerIndex); ApplyPlayer(true); }
    public void PlayerDown() { playerIndex = Next(playerIndex); ApplyPlayer(true); }

    public void ComUp() { comIndex = Prev(comIndex); ApplyCom(true); }
    public void ComDown() { comIndex = Next(comIndex); ApplyCom(true); }

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

        var c = playerPortrait.color;
        c.a = 1f;
        playerPortrait.color = c;

        if (animate) StartCoroutine(Pop(playerPortrait.rectTransform));
    }

    void ApplyCom(bool animate)
    {
        if (comPortrait == null || portraitSprites.Length == 0) return;

        comPortrait.sprite = portraitSprites[comIndex];

        var c = comPortrait.color;
        c.a = 1f;
        comPortrait.color = c;

        if (animate) StartCoroutine(Pop(comPortrait.rectTransform));
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

    public void ConfirmSelection()
    {
        if (SelectionData.Instance == null)
        {
            Debug.LogError("SelectionData.Instance missing!");
            return;
        }

        if (characterPrefabs != null && characterPrefabs.Length > 0)
        {
            SelectionData.Instance.playerPrefab = characterPrefabs[playerIndex];
            Debug.Log("Saved player prefab: " + SelectionData.Instance.playerPrefab.name);
        }

        SceneManager.LoadScene(gameSceneName);
    }

    void Start()
    {
        ApplyPlayer(false);
        ApplyCom(false);
    }
}