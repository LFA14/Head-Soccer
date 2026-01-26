using UnityEngine;

public class BallAntiStick : MonoBehaviour
{
    public float minUpBounce = 2.5f;

    void OnCollisionStay2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player")) return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb.linearVelocity.y < minUpBounce)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, minUpBounce);
    }
}
