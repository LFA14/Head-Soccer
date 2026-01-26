using UnityEngine;

public class UIFloat : MonoBehaviour
{
    public Vector2 moveAmount = new Vector2(0f, 15f); // pixels (UI)
    public float speed = 1.2f;

    RectTransform rt;
    Vector2 startPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    void Update()
    {
        float t = Mathf.Sin(Time.unscaledTime * speed);
        rt.anchoredPosition = startPos + moveAmount * t;
    }
}
