using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    [Header("Role")]
    public bool isAI = true;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public Transform bodyCenter;

    private Transform ball;
    private Rigidbody2D ballRb;
    private KickController kickController;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float airMoveSpeed = 6f;
    public float stopDistance = 0.2f;

    [Header("Jump")]
    public float jumpVelocity = 14f;
    public float jumpCooldown = 0.45f;
    public float minBallHeightToJump = 0.7f;
    public float jumpRangeX = 2.8f;

    [Header("Kick")]
    public float kickDistance = 2f;
    public float kickCooldown = 0.25f;

    [Header("AI Soccer Logic")]
    public bool attackRightGoal = true;
    public float behindBallOffset = 1.1f;
    public float defendRange = 8f;
    public float attackCommitDistance = 5f;
    public float predictionTime = 0.25f;

    [Header("Home Position")]
    public float homeX = -6f;

    private float nextJumpTime = 0f;
    private float nextKickTime = 0f;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        kickController = GetComponent<KickController>();

        if (kickController == null)
            kickController = GetComponentInParent<KickController>();

        if (bodyCenter == null)
            bodyCenter = transform;

        FindBall();
    }

    private void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!isAI)
            return;

        if (rb == null)
            return;

        if (ball == null)
        {
            FindBall();
            return;
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

        float myX = bodyCenter.position.x;
        float myY = bodyCenter.position.y;

        float predictedBallX = ball.position.x;
        float predictedBallY = ball.position.y;

        if (ballRb != null)
        {
            predictedBallX += ballRb.linearVelocity.x * predictionTime;
            predictedBallY += ballRb.linearVelocity.y * predictionTime;
        }

        float distanceToBallX = Mathf.Abs(predictedBallX - myX);

        bool ballIsFar = distanceToBallX > defendRange;
        bool ballNearMe = distanceToBallX < attackCommitDistance;

        float targetX;

        if (ballIsFar)
        {
            targetX = homeX;
        }
        else
        {
            if (attackRightGoal)
                targetX = predictedBallX - behindBallOffset;
            else
                targetX = predictedBallX + behindBallOffset;

            if (ballNearMe)
            {
                if (attackRightGoal)
                    targetX = predictedBallX - 0.4f;
                else
                    targetX = predictedBallX + 0.4f;
            }
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

        if (isGrounded && Time.time >= nextJumpTime && (shouldJump || emergencyJump))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            nextJumpTime = Time.time + jumpCooldown;
        }

        float distToBall = Vector2.Distance(bodyCenter.position, ball.position);

        if (distToBall < kickDistance && Time.time >= nextKickTime && kickController != null)
        {
            kickController.TriggerKick();
            nextKickTime = Time.time + kickCooldown;

            float burstX = attackRightGoal ? 10f : -10f;
            rb.linearVelocity = new Vector2(burstX, rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}