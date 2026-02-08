using UnityEngine;

public class KickController : MonoBehaviour
{
    public Transform legPivot;
    public Rigidbody2D playerRb;

    public float kickAngle = -60f;
    public float kickSpeed = 900f;
    public float returnSpeed = 1200f;
    public float walkSwingAngle = 18f;
    public float walkSwingSpeed = 12f;
    public float walkSpeedForMaxSwing = 7f;

    bool isKicking;

    void Update()
    {
        // K = kick
        if (Input.GetKeyDown(KeyCode.K))
            isKicking = true;

        float targetAngle = 0f;
        if (isKicking)
        {
            targetAngle = kickAngle;
        }
        else
        {
            float moveSpeed = 0f;
            if (playerRb != null)
                moveSpeed = Mathf.Abs(playerRb.linearVelocity.x);

            float swingT = walkSpeedForMaxSwing > 0f
                ? Mathf.Clamp01(moveSpeed / walkSpeedForMaxSwing)
                : 0f;

            if (swingT > 0f)
                targetAngle = Mathf.Sin(Time.time * walkSwingSpeed) * walkSwingAngle * swingT;
        }

        float currentAngle = legPivot.localEulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;

        float speed = isKicking ? kickSpeed : returnSpeed;
        float newAngle = Mathf.MoveTowards(
            currentAngle,
            targetAngle,
            speed * Time.deltaTime
        );

        legPivot.localRotation = Quaternion.Euler(0, 0, newAngle);

        if (isKicking && Mathf.Abs(newAngle - kickAngle) < 1f)
            isKicking = false;
    }
}
