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
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float airMoveSpeed = 6f;
    public float stopDistance = 0.3f;
    public float slowDownDistance = 1.2f;

    [Header("Jump")]
    public float jumpVelocity = 14f;
    public float jumpCooldown = 0.5f;
    public float minBallHeightToJump = 0.8f;
    public float jumpRangeX = 2.5f;
    public float emergencyJumpRangeX = 1.2f;

    [Header("Anti Juggle")]
    public float overheadTrapRangeX = 0.45f;
    public float overheadTrapHeight = 1.15f;
    public float overheadTrapMaxRiseSpeed = 1.5f;

    [Header("Kick")]
    public float kickDistance = 2f;
    public float kickCooldown = 0.3f;
    public float kickHeightTolerance = 1.5f;
    public float kickBurstSpeed = 8f;
    public float overheadKickRangeX = 0.8f;
    public float overheadKickMinHeight = 0.2f;
    public float overheadKickMaxHeight = 2f;
    public float kickPredictionTime = 0.08f;

    [Header("Soccer Logic")]
    public bool attackRightGoal = true;
    public float behindBallOffset = 1f;
    public float closeControlOffset = 0.35f;
    public float defendRange = 7f;
    public float attackCommitDistance = 4.5f;
    public float predictionTime = 0.2f;

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

        bool isGrounded = IsGrounded();

        Vector2 myPos = bodyCenter != null ? (Vector2)bodyCenter.position : (Vector2)transform.position;
        Vector2 ballPos = ball.position;
        Vector2 ballVel = ballRb != null ? ballRb.linearVelocity : Vector2.zero;

        Vector2 predictedBall = ballPos + ballVel * predictionTime;

        float myX = myPos.x;
        float myY = myPos.y;

        float distanceToBallX = Mathf.Abs(predictedBall.x - myX);
        float directBallX = Mathf.Abs(ballPos.x - myX);
        bool ballSettledOnHead =
            directBallX <= overheadTrapRangeX &&
            ballPos.y <= myY + overheadTrapHeight &&
            ballVel.y <= overheadTrapMaxRiseSpeed;

        float targetX;

        if (distanceToBallX > defendRange)
        {
            targetX = homeX;
        }
        else
        {
            if (attackRightGoal)
                targetX = predictedBall.x - behindBallOffset;
            else
                targetX = predictedBall.x + behindBallOffset;

            if (distanceToBallX < attackCommitDistance)
            {
                if (attackRightGoal)
                    targetX = predictedBall.x - closeControlOffset;
                else
                    targetX = predictedBall.x + closeControlOffset;
            }
        }

        MoveToTarget(targetX, isGrounded, myX);

        bool shouldJump =
            predictedBall.y > myY + minBallHeightToJump &&
            Mathf.Abs(predictedBall.x - myX) < jumpRangeX &&
            !ballSettledOnHead;

        bool emergencyJump =
            ballPos.y > myY + 0.15f &&
            Mathf.Abs(ballPos.x - myX) < emergencyJumpRangeX &&
            directBallX > overheadTrapRangeX;

        if (isGrounded && Time.time >= nextJumpTime && (shouldJump || emergencyJump))
        {
            SetVelocity(rb.linearVelocity.x, jumpVelocity);
            nextJumpTime = Time.time + jumpCooldown;
        }

        TryKick(myPos, ballPos);
    }

    private void MoveToTarget(float targetX, bool isGrounded, float myX)
    {
        float xToTarget = targetX - myX;
        float absX = Mathf.Abs(xToTarget);

        float targetSpeed = 0f;

        if (absX > stopDistance)
        {
            float dir = Mathf.Sign(xToTarget);
            float speed = isGrounded ? moveSpeed : airMoveSpeed;

            if (absX < slowDownDistance)
            {
                float factor = Mathf.Clamp(absX / slowDownDistance, 0.35f, 1f);
                speed *= factor;
            }

            targetSpeed = dir * speed;
        }

        SetVelocity(targetSpeed, rb.linearVelocity.y);
    }

    private void TryKick(Vector2 myPos, Vector2 ballPos)
    {
        if (kickController == null)
            return;

        if (Time.time < nextKickTime)
            return;

        Vector2 kickBallPos = ballPos;
        if (ballRb != null)
            kickBallPos += ballRb.linearVelocity * kickPredictionTime;

        float distToBall = Vector2.Distance(myPos, kickBallPos);
        float verticalDiff = Mathf.Abs(kickBallPos.y - myPos.y);
        float horizontalDiff = Mathf.Abs(kickBallPos.x - myPos.x);

        bool ballCloseEnough = distToBall <= kickDistance;
        bool heightOkay = verticalDiff <= kickHeightTolerance;
        bool ballInFront = attackRightGoal ? kickBallPos.x >= myPos.x - 0.2f : kickBallPos.x <= myPos.x + 0.2f;
        bool overheadClearKick =
            horizontalDiff <= overheadKickRangeX &&
            kickBallPos.y >= myPos.y + overheadKickMinHeight &&
            kickBallPos.y <= myPos.y + overheadKickMaxHeight;

        if (ballCloseEnough && ((heightOkay && ballInFront) || overheadClearKick))
        {
            kickController.TriggerKick();
            nextKickTime = Time.time + kickCooldown;

            float burstX = attackRightGoal ? kickBurstSpeed : -kickBurstSpeed;
            SetVelocity(burstX, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
            return true;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void SetVelocity(float x, float y)
    {
        rb.linearVelocity = new Vector2(x, y);
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
