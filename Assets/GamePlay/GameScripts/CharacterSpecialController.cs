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

    [Header("Aura Sprite Sheet (Optional)")]
    public SpriteRenderer fireAuraRenderer;
    public Sprite[] fireAuraFrames;
    public float fireAuraFps = 16f;
    public Vector3 fireAuraLocalOffset = new Vector3(0f, -0.1f, 0f);
    public int auraSortingOffsetFromPlayer = -1;
    public SpriteRenderer playerBodyRenderer;
    public Transform fireAuraFollowTarget;

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
    private float auraFrameTimer;
    private int auraFrameIndex;
    private bool auraSetupWarningShown;

    void Awake()
    {
        playerRb = GetComponentInChildren<Rigidbody2D>();
        AutoAssignAuraReferences();
        SetupAuraSortingAndPlacement();
        SetAuraSpriteVisible(false);
    }

    void Update()
    {
        if (!isPlayerControlled)
            return;

        bool canArm = linkedPowerBar == null || linkedPowerBar.IsFull;

        if (Input.GetKeyDown(activationKey) && canArm)
            SetSpecialArmed(true);

        if (specialArmed)
            UpdateAuraSpriteAnimation();
    }

    public void Configure(bool controlledByPlayer)
    {
        isPlayerControlled = controlledByPlayer;
        AssignPowerFromCharacterName(gameObject.name);
        linkedPowerBar = FindLinkedPowerBar();
        AttachRelaysToColliders();
        AutoAssignAuraReferences();
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

    public void ResetSpecialState()
    {
        SetSpecialArmed(false);
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

        if (specialArmed)
        {
            SetupAuraSortingAndPlacement();

            if (fireAuraEffect != null && !fireAuraEffect.isPlaying)
                fireAuraEffect.Play();

            SetAuraSpriteVisible(true);
            ValidateAuraSetup();
        }
        else
        {
            if (fireAuraEffect != null)
                fireAuraEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            SetAuraSpriteVisible(false);
        }
    }

    void UpdateAuraSpriteAnimation()
    {
        if (fireAuraRenderer == null || fireAuraFrames == null || fireAuraFrames.Length == 0)
            return;

        float safeFps = Mathf.Max(1f, fireAuraFps);
        float frameDuration = 1f / safeFps;
        auraFrameTimer += Time.deltaTime;

        while (auraFrameTimer >= frameDuration)
        {
            auraFrameTimer -= frameDuration;
            auraFrameIndex = (auraFrameIndex + 1) % fireAuraFrames.Length;
            fireAuraRenderer.sprite = fireAuraFrames[auraFrameIndex];
        }
    }

    void SetAuraSpriteVisible(bool visible)
    {
        if (fireAuraRenderer == null)
            return;

        if (!fireAuraRenderer.gameObject.activeSelf)
            fireAuraRenderer.gameObject.SetActive(true);

        fireAuraRenderer.enabled = visible;

        if (visible)
        {
            if (fireAuraFrames != null && fireAuraFrames.Length > 0)
            {
                auraFrameTimer = 0f;
                auraFrameIndex = 0;
                fireAuraRenderer.sprite = fireAuraFrames[auraFrameIndex];
            }

            fireAuraRenderer.transform.localPosition = fireAuraLocalOffset;
        }
    }

    void AutoAssignAuraReferences()
    {
        if (fireAuraRenderer == null)
        {
            Transform auraChild = transform.Find("fireAura");

            if (auraChild == null)
                auraChild = transform.Find("FireAura");

            if (auraChild != null)
                fireAuraRenderer = auraChild.GetComponent<SpriteRenderer>();
        }

        if (playerBodyRenderer == null)
            playerBodyRenderer = FindBestPlayerBodyRenderer();
    }

    void ValidateAuraSetup()
    {
        if (auraSetupWarningShown)
            return;

        if (fireAuraRenderer == null)
        {
            Debug.LogWarning($"[{name}] Fire aura not shown: FireAuraRenderer is not assigned.", this);
            auraSetupWarningShown = true;
            return;
        }

        if ((fireAuraFrames == null || fireAuraFrames.Length == 0) && fireAuraRenderer.sprite == null)
        {
            Debug.LogWarning($"[{name}] Fire aura not shown: FireAuraFrames is empty and FireAuraRenderer has no sprite.", this);
            auraSetupWarningShown = true;
        }
    }

    void SetupAuraSortingAndPlacement()
    {
        if (fireAuraRenderer == null)
            return;

        if (playerBodyRenderer == null)
            playerBodyRenderer = FindBestPlayerBodyRenderer();

        Transform followTarget = fireAuraFollowTarget;

        if (followTarget == null && playerBodyRenderer != null)
            followTarget = playerBodyRenderer.transform;

        if (followTarget == null && playerRb != null)
            followTarget = playerRb.transform;

        if (followTarget != null && fireAuraRenderer.transform.parent != followTarget)
            fireAuraRenderer.transform.SetParent(followTarget, false);

        fireAuraRenderer.transform.localPosition = fireAuraLocalOffset;

        int referenceLayerId = fireAuraRenderer.sortingLayerID;
        int referenceOrder = fireAuraRenderer.sortingOrder;

        if (playerBodyRenderer != null)
        {
            referenceLayerId = playerBodyRenderer.sortingLayerID;
            referenceOrder = playerBodyRenderer.sortingOrder;
        }
        else
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || renderers[i] == fireAuraRenderer)
                    continue;

                if (renderers[i].sortingOrder > referenceOrder)
                {
                    referenceOrder = renderers[i].sortingOrder;
                    referenceLayerId = renderers[i].sortingLayerID;
                }
            }
        }

        fireAuraRenderer.sortingLayerID = referenceLayerId;
        fireAuraRenderer.sortingOrder = referenceOrder + auraSortingOffsetFromPlayer;
    }

    SpriteRenderer FindBestPlayerBodyRenderer()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer best = null;
        int bestOrder = int.MinValue;

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer candidate = renderers[i];

            if (candidate == null || candidate == fireAuraRenderer)
                continue;

            if (candidate.sortingOrder > bestOrder)
            {
                bestOrder = candidate.sortingOrder;
                best = candidate;
            }
        }

        return best;
    }

    void OnDisable()
    {
        SetSpecialArmed(false);
    }
}
