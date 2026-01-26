using UnityEngine;

public class KickController : MonoBehaviour
{
    public Transform legPivot;

    public float kickAngle = -60f;
    public float kickSpeed = 900f;
    public float returnSpeed = 1200f;

    bool isKicking;

    void Update()
    {
        // K = kick
        if (Input.GetKeyDown(KeyCode.K))
            isKicking = true;

        float targetAngle = isKicking ? kickAngle : 0f;

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
