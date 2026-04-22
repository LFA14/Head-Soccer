using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character prefabs")]
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

    [Header("Arrow Sound")]
    public AudioSource arrowSfxSource;
    public AudioClip arrowClickSfx;
    [Range(0f, 1f)] public float arrowClickVolume = 1f;

    [Header("Scene")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        ApplyPlayer(false);
        ApplyCom(false);
    }

    public void PlayerUp()
    {
        playerIndex = Prev(playerIndex);
        ApplyPlayer(true);
        PlayArrowSound();
    }

    public void PlayerDown()
    {
        playerIndex = Next(playerIndex);
        ApplyPlayer(true);
        PlayArrowSound();
    }

    public void ComUp()
    {
        comIndex = Prev(comIndex);
        ApplyCom(true);
        PlayArrowSound();
    }

    public void ComDown()
    {
        comIndex = Next(comIndex);
        ApplyCom(true);
        PlayArrowSound();
    }

    int Next(int i)
    {
        if (portraitSprites == null || portraitSprites.Length == 0)
            return 0;

        return (i + 1) % portraitSprites.Length;
    }

    int Prev(int i)
    {
        if (portraitSprites == null || portraitSprites.Length == 0)
            return 0;

        return (i - 1 + portraitSprites.Length) % portraitSprites.Length;
    }

    void ApplyPlayer(bool animate)
    {
        if (playerPortrait == null || portraitSprites == null || portraitSprites.Length == 0)
            return;

        playerPortrait.sprite = portraitSprites[playerIndex];

        Color c = playerPortrait.color;
        c.a = 1f;
        playerPortrait.color = c;

        if (animate)
            StartCoroutine(Pop(playerPortrait.rectTransform));
    }

    void ApplyCom(bool animate)
    {
        if (comPortrait == null || portraitSprites == null || portraitSprites.Length == 0)
            return;

        comPortrait.sprite = portraitSprites[comIndex];

        Color c = comPortrait.color;
        c.a = 1f;
        comPortrait.color = c;

        if (animate)
            StartCoroutine(Pop(comPortrait.rectTransform));
    }

    IEnumerator Pop(RectTransform t)
    {
        if (t == null)
            yield break;

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

    void PlayArrowSound()
    {
        if (arrowSfxSource == null || arrowClickSfx == null)
            return;

        arrowSfxSource.PlayOneShot(arrowClickSfx, arrowClickVolume);
    }

    public void ConfirmSelection()
    {
        if (SelectionData.Instance == null)
        {
            Debug.LogError("SelectionData.Instance missing!");
            return;
        }

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Character prefabs array is empty!");
            return;
        }

        if (playerIndex < 0 || playerIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Player index is out of range!");
            return;
        }

        if (comIndex < 0 || comIndex >= characterPrefabs.Length)
        {
            Debug.LogError("COM index is out of range!");
            return;
        }

        SelectionData.Instance.playerPrefab = characterPrefabs[playerIndex];
        SelectionData.Instance.comPrefab = characterPrefabs[comIndex];

        if (MatchContext.Instance != null)
            MatchContext.Instance.SetMode(MatchContext.MatchMode.QuickMatch);

        Debug.Log("Saved player prefab: " + SelectionData.Instance.playerPrefab.name);
        Debug.Log("Saved com prefab: " + SelectionData.Instance.comPrefab.name);

        SceneManager.LoadScene(gameSceneName);
    }
}
