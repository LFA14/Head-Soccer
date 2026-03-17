using UnityEngine;

public class KickController : MonoBehaviour
{
    public Transform legPivot;
    public Rigidbody2D playerRb;

    public float kickAngle = -60f;
    public float kickSpeed = 900f;
    public float returnSpeed = 1200f;

    public float moveAngle = 20f;
    public float minMoveSpeed = 0.1f;

    public bool isPlayer = true;

    private bool isKicking;

    void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.K))
        {
            isKicking = true;
        }

        float targetAngle = 0f;

        if (isKicking)
        {
            targetAngle = kickAngle;
        }
        else
        {
            float speedX = 0f;

            if (playerRb != null)
                speedX = playerRb.linearVelocity.x;

            if (speedX > minMoveSpeed)
                targetAngle = moveAngle;
            else if (speedX < -minMoveSpeed)
                targetAngle = -moveAngle;
        }

        if (legPivot == null) return;

        float currentAngle = legPivot.localEulerAngles.z;

        if (currentAngle > 180f)
            currentAngle -= 360f;

        float speed = isKicking ? kickSpeed : returnSpeed;

        float newAngle = Mathf.MoveTowards(
            currentAngle,
            targetAngle,
            speed * Time.deltaTime
        );

        legPivot.localRotation = Quaternion.Euler(0f, 0f, newAngle);

        if (isKicking && Mathf.Abs(newAngle - kickAngle) < 1f)
            isKicking = false;
    }

    public void TriggerKick()
    {
        isKicking = true;
    }
}