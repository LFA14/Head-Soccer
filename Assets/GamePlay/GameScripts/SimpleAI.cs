using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    public Rigidbody2D rb;
    private Transform ball;

    public float moveSpeed = 4f;
    public float jumpForce = 10f;
    public float kickDistance = 1.5f;

    public KickController kickController;

    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    public float jumpCooldown = 1f;
    private float nextJumpTime = 0f;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // automatically find KickController on this object or parent
        kickController = GetComponent<KickController>();

        if (kickController == null)
            kickController = GetComponentInParent<KickController>();

        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null)
            ball = ballObj.transform;
    }

    void Update()
    {
        if (ball == null || rb == null) return;

        float dx = ball.position.x - transform.position.x;

        if (Mathf.Abs(dx) > 0.2f)
        {
            float dir = Mathf.Sign(dx);
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        if (
            isGrounded &&
            Time.time >= nextJumpTime &&
            ball.position.y > transform.position.y + 1f
        )
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            nextJumpTime = Time.time + jumpCooldown;
        }

        float dist = Vector2.Distance(transform.position, ball.position);

        if (dist < kickDistance)
        {
            if (kickController != null)
                kickController.TriggerKick();
        }
    }
}