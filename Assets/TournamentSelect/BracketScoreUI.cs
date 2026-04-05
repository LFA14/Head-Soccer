using System.Collections;
using TMPro;
using UnityEngine;

public class BracketScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public float popScale = 1.18f;
    public float popSpeed = 14f;

    Coroutine routine;

    public void Show(string text)
    {
        if (scoreText == null) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(text));
    }

    public void ShowImmediate(string text)
    {
        if (scoreText == null) return;

        if (routine != null) StopCoroutine(routine);
        routine = null;
        scoreText.gameObject.SetActive(true);
        scoreText.text = text;
        scoreText.alpha = 1f;
        scoreText.rectTransform.localScale = Vector3.one;
    }

    IEnumerator ShowRoutine(string text)
    {
        scoreText.gameObject.SetActive(true);
        scoreText.text = text;

        // start visible + pop in
        scoreText.alpha = 1f;
        scoreText.rectTransform.localScale = Vector3.one * 0.9f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * popSpeed;
            scoreText.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one * popScale, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * popSpeed;
            scoreText.rectTransform.localScale = Vector3.Lerp(Vector3.one * popScale, Vector3.one, t);
            yield return null;
        }

        // ✅ IMPORTANT: do NOT hide or fade out anymore
        routine = null;
    }

    public void Hide()
    {
        if (scoreText == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = null;
        scoreText.gameObject.SetActive(false);
    }
}
