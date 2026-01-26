using UnityEngine;

public class UIRotate : MonoBehaviour
{
    public float speed = 180f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, 0f, -speed * Time.unscaledDeltaTime);
    }
}
