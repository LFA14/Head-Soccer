using UnityEngine;

public class BootKick : MonoBehaviour
{
    public KickController kickController;
    public float extraKickPower = 12f;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (kickController == null) return;

        Rigidbody2D ballRb = col.rigidbody;
        if (ballRb == null) return;

        // Only apply extra force while kick is happening
        // (simple check: leg is not at rest)
        float z = kickController.legPivot.localEulerAngles.z;
        if (z < 5f || z > 355f) return;

        Vector2 dir = (ballRb.position - (Vector2)transform.position).normalized;
        ballRb.AddForce(dir * extraKickPower, ForceMode2D.Impulse);
    }
}
