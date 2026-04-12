using System.Collections;
using UnityEngine;

public class BallSpecialVisualController : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private Coroutine restoreRoutine;

    void Awake()
    {
        CacheRenderers();
    }

    public void SetTint(Color tint)
    {
        CacheRenderers();

        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = tint;
        }
    }

    public void SetTintForDuration(Color tint, float duration)
    {
        SetTint(tint);

        if (duration <= 0f)
        {
            RestoreOriginal();
            return;
        }

        restoreRoutine = StartCoroutine(RestoreAfterDelay(duration));
    }

    public void RestoreOriginal()
    {
        CacheRenderers();

        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalColors.Length)
                renderers[i].color = originalColors[i];
        }
    }

    IEnumerator RestoreAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        restoreRoutine = null;
        RestoreOriginal();
    }

    void CacheRenderers()
    {
        if (renderers != null && originalColors != null && renderers.Length == originalColors.Length)
            return;

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }
    }

    void OnDisable()
    {
        RestoreOriginal();
    }
}
