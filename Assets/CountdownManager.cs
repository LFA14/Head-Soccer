using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour
{
    public Image countdownImage;

    public Sprite threeSprite;
    public Sprite twoSprite;
    public Sprite oneSprite;

    public GameObject[] objectsToDisableAtStart;

    void Start()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        SetGameplay(false);

        countdownImage.gameObject.SetActive(true);

        // 3
        yield return ShowNumber(threeSprite);

        // 2
        yield return ShowNumber(twoSprite);

        // 1
        yield return ShowNumber(oneSprite);

        // Hide countdown
        countdownImage.gameObject.SetActive(false);

        SetGameplay(true);
    }

    IEnumerator ShowNumber(Sprite sprite)
    {
        countdownImage.sprite = sprite;

        // 🔥 Force UI refresh (fixes "1 not showing")
        yield return null;

        // 🔥 Pop animation
        yield return StartCoroutine(PopEffect());

        // Wait before next number
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator PopEffect()
    {
        float duration = 0.25f;
        float t = 0;

        Vector3 start = Vector3.zero;
        Vector3 peak = Vector3.one * 1.3f;
        Vector3 end = Vector3.one;

        countdownImage.transform.localScale = start;

        // Scale up
        while (t < duration)
        {
            t += Time.deltaTime;
            countdownImage.transform.localScale = Vector3.Lerp(start, peak, t / duration);
            yield return null;
        }

        t = 0;

        // Scale back down
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
        foreach (GameObject obj in objectsToDisableAtStart)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
            else
            {
                Debug.LogWarning("Missing object in disable list!");
            }
        }
    }
}