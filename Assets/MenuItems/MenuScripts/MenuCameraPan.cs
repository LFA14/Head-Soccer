using UnityEngine;

public class MenuCameraPan : MonoBehaviour
{
    public Vector3 offset = new Vector3(0.4f, 0.2f, 0f);
    public float speed = 0.25f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0..1
        transform.position = Vector3.Lerp(startPos - offset, startPos + offset, t);
    }
}
