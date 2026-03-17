using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    [Header("Role")]
    public bool isAI = false;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform groundCheck;

    private Transform ball;
    private Rigidbody2D ballRb;
    private KickController kickController;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 9f;
    public float airMoveSpeed = 6f;
    public float stopDistance = 0.15f;

    [Header("Jump")]
    public float jumpVelocity = 14f;
    public float jumpCooldown = 0.45f;
    public float minBallHeightToJump = 0.7f;
    public float jumpRangeX = 2.8f;

    [Header("Kick")]
    public float kickDistance = 2f;
    public float kickCooldown = 0.25f;

    [Header("AI Soccer Logic")]
    public float behindBallOffset = 1.1f;
    public float defendRange = 8f;
    public float attackCommitDistance = 5f;
    public float predictionTime = 0.25f;

    [Header("Home Position")]
    public float homeX = -6f;

    private float nextJumpTime = 0f;
    private float nextKickTime = 0f;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        kickController = GetComponent<KickController>();

        if (kickController == null)
            kickController = GetComponentInParent<KickController>();

        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (!isAI) return;
        if (rb == null || ball == null) return;

        bool isGrounded = false;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        float myX = transform.position.x;
        float myY = transform.position.y;

        float predictedBallX = ball.position.x;
        float predictedBallY = ball.position.y;

        if (ballRb != null)
        {
            predictedBallX += ballRb.linearVelocity.x * predictionTime;
            predictedBallY += ballRb.linearVelocity.y * predictionTime;
        }

        float distanceToBallX = Mathf.Abs(predictedBallX - myX);

        float targetX;
        bool ballIsFar = distanceToBallX > defendRange;
        bool ballNearMe = distanceToBallX < attackCommitDistance;

        // AI attacks RIGHT goal
        if (ballIsFar)
        {
            targetX = homeX;
        }
        else
        {
            targetX = predictedBallX - behindBallOffset;

            if (ballNearMe)
                targetX = predictedBallX - 0.4f;
        }

        float xToTarget = targetX - myX;
        float targetSpeed = 0f;

        if (Mathf.Abs(xToTarget) > stopDistance)
        {
            float dir = Mathf.Sign(xToTarget);
            targetSpeed = dir * (isGrounded ? moveSpeed : airMoveSpeed);
        }

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        bool shouldJump =
            predictedBallY > myY + minBallHeightToJump &&
            Mathf.Abs(predictedBallX - myX) < jumpRangeX;

        bool emergencyJump =
            ball.position.y > myY + 0.2f &&
            Mathf.Abs(ball.position.x - myX) < 1.2f;

        if (
            isGrounded &&
            Time.time >= nextJumpTime &&
            (shouldJump || emergencyJump)
        )
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            nextJumpTime = Time.time + jumpCooldown;
        }

        float distToBall = Vector2.Distance(transform.position, ball.position);

        if (
            distToBall < kickDistance &&
            Time.time >= nextKickTime &&
            kickController != null
        )
        {
            kickController.TriggerKick();
            nextKickTime = Time.time + kickCooldown;

            // burst toward RIGHT goal
            rb.linearVelocity = new Vector2(10f, rb.linearVelocity.y);
        }
    }
}