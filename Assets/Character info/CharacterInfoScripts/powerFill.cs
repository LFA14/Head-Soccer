using UnityEngine;
using UnityEngine.UI;

public class PowerFill : MonoBehaviour
{
    public Image fillImage;
    public float fillTime = 5f;

    public RectTransform barTransform;

    public float shakeAmount = 5f;
    public float shakeSpeed = 20f;

    public float glowSpeed = 4f;

    float timer = 0;
    bool full = false;
    Vector3 originalPos;
    Color baseColor;
    bool initialized;

    public bool IsFull
    {
        get { return full; }
    }

    public float NormalizedProgress
    {
        get
        {
            if (full)
                return 1f;

            if (fillTime <= 0f)
                return 1f;

            return Mathf.Clamp01(timer / fillTime);
        }
    }

    void Awake()
    {
        CacheInitialState();
    }

    void Start()
    {
        CacheInitialState();
        ResetBarInstant();
    }

    void CacheInitialState()
    {
        if (initialized)
            return;

        if (barTransform == null)
            barTransform = GetComponent<RectTransform>();

        if (barTransform != null)
            originalPos = barTransform.localPosition;

        if (fillImage != null)
            baseColor = fillImage.color;

        initialized = true;
    }

    void Update()
    {
        if (fillImage == null)
            return;

        if (!full)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(timer / fillTime);

            if (fillImage.fillAmount >= 1f)
            {
                full = true;
            }
        }
        else
        {
            // SHAKE
            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float y = Mathf.Cos(Time.time * shakeSpeed) * shakeAmount;
            if (barTransform != null)
                barTransform.localPosition = originalPos + new Vector3(x, y, 0);

            // GLOW
            float glow = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;
            fillImage.color = Color.Lerp(baseColor, baseColor * 2f, glow);
        }
    }

    public bool TryConsume()
    {
        if (!full)
            return false;

        ResetBarInstant();
        return true;
    }

    public void ResetBarInstant()
    {
        CacheInitialState();

        timer = 0f;
        full = false;

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = baseColor;
        }

        if (barTransform != null)
            barTransform.localPosition = originalPos;
    }

    public void SetProgressNormalized(float normalizedProgress)
    {
        CacheInitialState();

        float clampedProgress = Mathf.Clamp01(normalizedProgress);
        full = clampedProgress >= 1f;
        timer = full ? fillTime : clampedProgress * Mathf.Max(fillTime, 0.0001f);

        if (fillImage != null)
        {
            fillImage.fillAmount = clampedProgress;
            fillImage.color = baseColor;
        }

        if (barTransform != null)
            barTransform.localPosition = originalPos;
    }

    public static void ResetAllBarsInScene()
    {
        PowerFill[] bars = FindObjectsOfType<PowerFill>(true);

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] != null)
                bars[i].ResetBarInstant();
        }
    }

    public static void ResetBarsForSide(bool rightSide)
    {
        PowerFill[] bars = FindObjectsOfType<PowerFill>(true);

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            RectTransform rect = bars[i].barTransform != null
                ? bars[i].barTransform
                : bars[i].GetComponent<RectTransform>();

            if (rect == null)
                continue;

            bool isRightBar = rect.anchoredPosition.x > 0f;
            if (isRightBar == rightSide)
                bars[i].ResetBarInstant();
        }
    }
}
