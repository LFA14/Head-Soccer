using System.Collections;
using UnityEngine;

public class CharacterSpecialController : MonoBehaviour
{
    public enum SpecialPowerType
    {
        PowerShot,
        CurveShot,
        DashShot,
        StallShot
    }

    [Header("Input")]
    public KeyCode activationKey = KeyCode.P;
    public bool isPlayerControlled = true;
    public PowerFill linkedPowerBar;
    public ParticleSystem fireAuraEffect;

    [Header("State")]
    public SpecialPowerType specialPower = SpecialPowerType.PowerShot;
    public bool specialArmed;

    [Header("Power Shot")]
    public float powerShotHorizontalForce = 45f;
    public float powerShotVerticalForce = 12f;

    [Header("Curve Shot")]
    public float curveShotHorizontalForce = 13f;
    public float curveShotVerticalForce = 8f;
    public float curveShotTorque = -14f;

    [Header("Dash Shot")]
    public float dashShotHorizontalForce = 12f;
    public float dashShotVerticalForce = 5f;
    public float playerDashForce = 7f;

    [Header("Stall Shot")]
    public float stallDuration = 0.2f;
    public float stallReleaseHorizontalForce = 10f;
    public float stallReleaseVerticalForce = 9f;

    [Header("Safety")]
    public float retriggerDelay = 0.15f;

    private Rigidbody2D playerRb;
    private float lastTriggerTime = -10f;

    void Awake()
    {
        playerRb = GetComponentInChildren<Rigidbody2D>();
    }

    void Update()
    {
        if (!isPlayerControlled)
            return;

        if (Input.GetKeyDown(activationKey) && linkedPowerBar != null && linkedPowerBar.IsFull)
            SetSpecialArmed(true);
    }

    public void Configure(bool controlledByPlayer)
    {
        isPlayerControlled = controlledByPlayer;
        AssignPowerFromCharacterName(gameObject.name);
        linkedPowerBar = FindLinkedPowerBar();
        AttachRelaysToColliders();
    }

    public void TryTriggerSpecial(Rigidbody2D ballRb)
    {
        if (ballRb == null || !specialArmed)
            return;

        if (Time.time - lastTriggerTime < retriggerDelay)
            return;

        if (linkedPowerBar != null && !linkedPowerBar.IsFull)
            return;

        bool isOnRightSide = transform.position.x > 0f;

        if (isPlayerControlled && MatchContext.Instance != null)
            isOnRightSide = MatchContext.Instance.playerIsOnRightSide;

        PowerFill.ResetBarsForSide(isOnRightSide);
        SetSpecialArmed(false);
        lastTriggerTime = Time.time;

        Vector2 shotDirection = GetAttackDirection();

        switch (specialPower)
        {
            case SpecialPowerType.PowerShot:
                ApplyPowerShot(ballRb, shotDirection);
                break;

            case SpecialPowerType.CurveShot:
                ApplyCurveShot(ballRb, shotDirection);
                break;

            case SpecialPowerType.DashShot:
                ApplyDashShot(ballRb, shotDirection);
                break;

            case SpecialPowerType.StallShot:
                StartCoroutine(ApplyStallShot(ballRb, shotDirection));
                break;
        }
    }

    void AssignPowerFromCharacterName(string characterName)
    {
        string normalizedName = characterName.ToLowerInvariant();

        if (normalizedName.Contains("argentina"))
        {
            specialPower = SpecialPowerType.PowerShot;
        }
        else if (normalizedName.Contains("brazil"))
        {
            specialPower = SpecialPowerType.CurveShot;
        }
        else if (normalizedName.Contains("egypt"))
        {
            specialPower = SpecialPowerType.DashShot;
        }
        else if (normalizedName.Contains("saudi"))
        {
            specialPower = SpecialPowerType.StallShot;
        }
    }

    void AttachRelaysToColliders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterSpecialTouchRelay relay = colliders[i].GetComponent<CharacterSpecialTouchRelay>();

            if (relay == null)
                relay = colliders[i].gameObject.AddComponent<CharacterSpecialTouchRelay>();

            relay.owner = this;
        }
    }

    PowerFill FindLinkedPowerBar()
    {
        PowerFill[] bars = FindObjectsOfType<PowerFill>(true);
        PowerFill bestMatch = null;
        float bestScore = float.MaxValue;
        bool wantRightSideBar = false;

        if (isPlayerControlled && MatchContext.Instance != null)
            wantRightSideBar = MatchContext.Instance.playerIsOnRightSide;
        else
            wantRightSideBar = transform.position.x > 0f;

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            RectTransform rect = bars[i].GetComponent<RectTransform>();
            if (rect == null)
                continue;

            string barName = bars[i].gameObject.name.ToLowerInvariant();
            if (!barName.Contains("mask"))
                continue;

            bool isRightBar = rect.anchoredPosition.x > 0f;
            if (isRightBar != wantRightSideBar)
                continue;

            float score = Mathf.Abs(rect.anchoredPosition.x);
            if (score < bestScore)
            {
                bestScore = score;
                bestMatch = bars[i];
            }
        }

        return bestMatch;
    }

    Vector2 GetAttackDirection()
    {
        if (isPlayerControlled && MatchContext.Instance != null)
            return MatchContext.Instance.playerIsOnRightSide ? Vector2.left : Vector2.right;

        return transform.position.x <= 0f ? Vector2.right : Vector2.left;
    }

    void ApplyPowerShot(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.AddForce(
            new Vector2(shotDirection.x * powerShotHorizontalForce, powerShotVerticalForce),
            ForceMode2D.Impulse
        );
    }

    void ApplyCurveShot(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        ballRb.linearVelocity = Vector2.zero;
        ballRb.AddForce(
            new Vector2(shotDirection.x * curveShotHorizontalForce, curveShotVerticalForce),
            ForceMode2D.Impulse
        );
        ballRb.AddTorque(curveShotTorque * Mathf.Sign(shotDirection.x), ForceMode2D.Impulse);
    }

    void ApplyDashShot(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        if (playerRb != null)
        {
            playerRb.AddForce(
                new Vector2(shotDirection.x * playerDashForce, 1.5f),
                ForceMode2D.Impulse
            );
        }

        ballRb.linearVelocity = Vector2.zero;
        ballRb.AddForce(
            new Vector2(shotDirection.x * dashShotHorizontalForce, dashShotVerticalForce),
            ForceMode2D.Impulse
        );
    }

    IEnumerator ApplyStallShot(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        Vector2 savedVelocity = ballRb.linearVelocity;
        float savedAngularVelocity = ballRb.angularVelocity;
        float savedGravity = ballRb.gravityScale;

        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.gravityScale = 0f;

        yield return new WaitForSeconds(stallDuration);

        if (ballRb == null)
            yield break;

        ballRb.gravityScale = savedGravity;
        ballRb.linearVelocity = savedVelocity * 0.2f;
        ballRb.angularVelocity = savedAngularVelocity * 0.2f;
        ballRb.AddForce(
            new Vector2(shotDirection.x * stallReleaseHorizontalForce, stallReleaseVerticalForce),
            ForceMode2D.Impulse
        );
    }

    void SetSpecialArmed(bool armed)
    {
        specialArmed = armed;

        if (fireAuraEffect == null)
            return;

        if (specialArmed)
        {
            if (!fireAuraEffect.isPlaying)
                fireAuraEffect.Play();
        }
        else
        {
            fireAuraEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnDisable()
    {
        SetSpecialArmed(false);
    }
}
