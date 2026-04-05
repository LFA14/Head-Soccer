using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentResultSequenceUI : MonoBehaviour
{
    [Header("UI References")]
    public Image resultIcon;
    public GameObject coinsGroup;
    public Image coinIcon;
    public TMP_Text coinsText;
    public Image messageIcon;
    public GameObject continueButton;

    [Header("Sprites")]
    public Sprite winSprite;
    public Sprite lossSprite;
    public Sprite congratsSprite;
    public Sprite hardLuckSprite;

    [Header("Timing")]
    public float firstDelay = 0.4f;
    public float betweenDelay = 0.35f;
    public float popDuration = 0.35f;
    public float coinsCountDuration = 1.2f;

    [Header("Scale")]
    public float startScale = 0.6f;
    public float endScale = 1f;
    public float overshootScale = 1.15f;

    private void Start()
    {
        PrepareUI();
        StartCoroutine(PlaySequence());
    }

    void PrepareUI()
    {
        if (resultIcon != null)
        {
            resultIcon.gameObject.SetActive(false);
            resultIcon.transform.localScale = Vector3.one * startScale;
            SetImageAlpha(resultIcon, 0f);
        }

        if (coinsGroup != null)
        {
            coinsGroup.SetActive(false);
            coinsGroup.transform.localScale = Vector3.one * startScale;
        }

        if (coinIcon != null)
            SetImageAlpha(coinIcon, 0f);

        if (coinsText != null)
        {
            coinsText.text = "0";
            SetTextAlpha(coinsText, 0f);
        }

        if (messageIcon != null)
        {
            messageIcon.gameObject.SetActive(false);
            messageIcon.transform.localScale = Vector3.one * startScale;
            SetImageAlpha(messageIcon, 0f);
        }

        if (continueButton != null)
            continueButton.SetActive(false);
    }

    IEnumerator PlaySequence()
    {
        if (TournamentResultData.Instance == null)
            yield break;

        var data = TournamentResultData.Instance;

        yield return new WaitForSeconds(firstDelay);

        if (resultIcon != null)
        {
            resultIcon.sprite = data.playerWon ? winSprite : lossSprite;
            yield return StartCoroutine(PopInImage(resultIcon));
        }

        yield return new WaitForSeconds(betweenDelay);

        if (coinsGroup != null)
        {
            coinsGroup.SetActive(true);
            yield return StartCoroutine(PopInCoinsGroup());
            yield return StartCoroutine(CountCoins(data.rewardCoins));
        }

        yield return new WaitForSeconds(betweenDelay);

        if (messageIcon != null)
        {
            messageIcon.sprite = data.playerWon ? congratsSprite : hardLuckSprite;
            yield return StartCoroutine(PopInImage(messageIcon));
        }

        yield return new WaitForSeconds(0.25f);

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    IEnumerator PopInImage(Image img)
    {
        img.gameObject.SetActive(true);

        float time = 0f;
        while (time < popDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popDuration);

            float scale = Mathf.Lerp(startScale, overshootScale, t);
            img.transform.localScale = Vector3.one * scale;
            SetImageAlpha(img, t);

            yield return null;
        }

        time = 0f;
        Vector3 from = Vector3.one * overshootScale;
        Vector3 to = Vector3.one * endScale;

        while (time < popDuration * 0.5f)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / (popDuration * 0.5f));
            img.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        img.transform.localScale = Vector3.one * endScale;
        SetImageAlpha(img, 1f);
    }

    IEnumerator PopInCoinsGroup()
    {
        float time = 0f;

        while (time < popDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popDuration);

            float scale = Mathf.Lerp(startScale, overshootScale, t);
            coinsGroup.transform.localScale = Vector3.one * scale;

            if (coinIcon != null)
                SetImageAlpha(coinIcon, t);

            if (coinsText != null)
                SetTextAlpha(coinsText, t);

            yield return null;
        }

        time = 0f;
        Vector3 from = Vector3.one * overshootScale;
        Vector3 to = Vector3.one * endScale;

        while (time < popDuration * 0.5f)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / (popDuration * 0.5f));
            coinsGroup.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        coinsGroup.transform.localScale = Vector3.one * endScale;

        if (coinIcon != null)
            SetImageAlpha(coinIcon, 1f);

        if (coinsText != null)
            SetTextAlpha(coinsText, 1f);
    }

    IEnumerator CountCoins(int targetCoins)
    {
        float time = 0f;

        while (time < coinsCountDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / coinsCountDuration);

            int currentCoins = Mathf.RoundToInt(Mathf.Lerp(0f, targetCoins, t));
            if (coinsText != null)
                coinsText.text = currentCoins.ToString();

            yield return null;
        }

        if (coinsText != null)
            coinsText.text = targetCoins.ToString();
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SetTextAlpha(TMP_Text txt, float alpha)
    {
        Color c = txt.color;
        c.a = alpha;
        txt.color = c;
    }
}