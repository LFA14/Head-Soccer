using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 4f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
{
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
}

rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

    }
}
