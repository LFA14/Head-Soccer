using UnityEngine;

public class BallAntiStick : MonoBehaviour
{
    public float minUpBounce = 2.5f;
    public float impactBounceMultiplier = 0.9f;
    public float extraBounceBoost = 0.35f;
    public float maxUpBounce = 8f;
    [Range(0f, 1f)] public float minTopContactNormalY = 0.35f;

    void OnCollisionEnter2D(Collision2D col)
    {
        ApplyCharacterBounce(col, useImpactBounce: true);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        ApplyCharacterBounce(col, useImpactBounce: false);
    }

    void ApplyCharacterBounce(Collision2D col, bool useImpactBounce)
    {
        if (!IsCharacterCollision(col) || !TryGetTopContactNormal(col, out Vector2 contactNormal))
            return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        float targetUpBounce = minUpBounce;

        if (useImpactBounce)
        {
            float impactSpeed = Mathf.Abs(Vector2.Dot(col.relativeVelocity, contactNormal));
            targetUpBounce = Mathf.Clamp(
                impactSpeed * impactBounceMultiplier + extraBounceBoost,
                minUpBounce,
                maxUpBounce
            );
        }

        if (rb.linearVelocity.y < targetUpBounce)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, targetUpBounce);
    }

    bool IsCharacterCollision(Collision2D col)
    {
        if (col.collider == null)
            return false;

        Transform hit = col.collider.transform;
        return
            hit.GetComponentInParent<PlayerMovement>() != null ||
            hit.GetComponentInParent<SimpleAI>() != null ||
            hit.GetComponentInParent<KickController>() != null;
    }

    bool TryGetTopContactNormal(Collision2D col, out Vector2 contactNormal)
    {
        contactNormal = Vector2.zero;

        for (int i = 0; i < col.contactCount; i++)
        {
            Vector2 normal = col.GetContact(i).normal;
            if (normal.y > minTopContactNormalY)
            {
                contactNormal = normal.normalized;
                return true;
            }
        }

        return false;
    }
}
