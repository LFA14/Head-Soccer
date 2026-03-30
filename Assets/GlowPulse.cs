using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public float speed = 3f;
    public float scaleAmount = 0.1f;

    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = originalScale * scale;
    }
}