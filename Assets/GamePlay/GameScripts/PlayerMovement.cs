using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4.5f;
    public float jumpForce = 20f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    public bool isPlayer = true;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isPlayer)
        {
            return;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");

        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        float targetX = moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
    }
}