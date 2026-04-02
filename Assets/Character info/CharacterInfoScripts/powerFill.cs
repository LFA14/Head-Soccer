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

    void Start()
    {
        originalPos = barTransform.localPosition;
        baseColor = fillImage.color;
    }

    void Update()
    {
        if (!full)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = timer / fillTime;

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
            barTransform.localPosition = originalPos + new Vector3(x, y, 0);

            // GLOW
            float glow = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;
            fillImage.color = Color.Lerp(baseColor, baseColor * 2f, glow);
        }
    }
}