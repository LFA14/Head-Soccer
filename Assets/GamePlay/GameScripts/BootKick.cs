using UnityEngine;

public class BootKick : MonoBehaviour
{
    public KickController kickController;
    public float extraKickPower = 12f;
    public float playerPushPower = 0.4f;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (kickController == null) return;

        float z = kickController.legPivot.localEulerAngles.z;
        if (z < 5f || z > 355f) return;

        Rigidbody2D hitRb = col.rigidbody;
        if (hitRb == null) return;

        if (col.gameObject.CompareTag("Ball"))
        {
            Vector2 dir = (hitRb.position - (Vector2)transform.position).normalized;
            hitRb.AddForce(dir * extraKickPower, ForceMode2D.Impulse);
        }
        else if (col.gameObject.CompareTag("Player"))
        {
            Vector2 dir = (hitRb.position - (Vector2)transform.position).normalized;
            hitRb.AddForce(dir * playerPushPower, ForceMode2D.Impulse);
        }
    }
}