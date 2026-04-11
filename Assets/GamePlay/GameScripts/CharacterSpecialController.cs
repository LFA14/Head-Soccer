using System.Collections;
using UnityEngine;

public class CharacterSpecialController : MonoBehaviour
{
    public enum SpecialPowerType
    {
        PowerShot,
        CurveShot,
        FreezeOpponent,
        StickyBall
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

    [Header("Freeze Opponent")]
    public float freezeDuration = 3f;
    public Color freezeTint = new Color(0.35f, 0.7f, 1f, 1f);

    [Header("Sticky Ball")]
    public float stickyBallDuration = 0.75f;
    public Vector2 stickyBallHoldOffset = new Vector2(0.9f, 0.35f);
    public float stickyBallReleaseHorizontalForce = 8f;
    public float stickyBallReleaseVerticalForce = 5f;

    [Header("Safety")]
    public float retriggerDelay = 0.15f;

    private Rigidbody2D playerRb;
    private Coroutine freezeRoutine;
    private Coroutine stickyBallRoutine;
    private Rigidbody2D stickyBallRb;
    private float stickyBallSavedGravity;
    private Collider2D[] stickyBallColliders;
    private Collider2D[] stickyOwnerColliders;
    private Transform frozenOpponentRoot;
    private Rigidbody2D frozenOpponentRb;
    private PlayerMovement frozenOpponentMovement;
    private KickController frozenOpponentKick;
    private SimpleAI frozenOpponentAI;
    private SpriteRenderer[] frozenOpponentRenderers;
    private Color[] frozenOpponentColors;
    private bool frozenOpponentMovementWasEnabled;
    private bool frozenOpponentKickWasEnabled;
    private bool frozenOpponentAIWasEnabled;
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

            case SpecialPowerType.FreezeOpponent:
                TriggerFreezeOpponent();
                break;

            case SpecialPowerType.StickyBall:
                TriggerStickyBall(ballRb, shotDirection);
                break;
        }
    }

    public void ResetSpecialState()
    {
        RestoreStickyBall();
        RestoreFrozenOpponent();
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
            specialPower = SpecialPowerType.FreezeOpponent;
        }
        else if (normalizedName.Contains("saudi"))
        {
            specialPower = SpecialPowerType.StickyBall;
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

    void TriggerFreezeOpponent()
    {
        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        RestoreFrozenOpponent();
        freezeRoutine = StartCoroutine(ApplyFreezeOpponent());
    }

    IEnumerator ApplyFreezeOpponent()
    {
        Transform opponentRoot = FindOpponentRoot();
        if (opponentRoot == null)
        {
            freezeRoutine = null;
            yield break;
        }

        frozenOpponentRoot = opponentRoot;
        frozenOpponentRb = opponentRoot.GetComponentInChildren<Rigidbody2D>(true);
        frozenOpponentMovement = opponentRoot.GetComponentInChildren<PlayerMovement>(true);
        frozenOpponentKick = opponentRoot.GetComponentInChildren<KickController>(true);
        frozenOpponentAI = opponentRoot.GetComponentInChildren<SimpleAI>(true);
        frozenOpponentRenderers = opponentRoot.GetComponentsInChildren<SpriteRenderer>(true);
        frozenOpponentColors = new Color[frozenOpponentRenderers.Length];

        if (frozenOpponentMovement != null)
        {
            frozenOpponentMovementWasEnabled = frozenOpponentMovement.enabled;
            frozenOpponentMovement.enabled = false;
        }

        if (frozenOpponentKick != null)
        {
            frozenOpponentKickWasEnabled = frozenOpponentKick.enabled;
            frozenOpponentKick.enabled = false;
        }

        if (frozenOpponentAI != null)
        {
            frozenOpponentAIWasEnabled = frozenOpponentAI.enabled;
            frozenOpponentAI.enabled = false;
        }

        if (frozenOpponentRb != null)
        {
            frozenOpponentRb.linearVelocity = Vector2.zero;
            frozenOpponentRb.angularVelocity = 0f;
        }

        for (int i = 0; i < frozenOpponentRenderers.Length; i++)
        {
            if (frozenOpponentRenderers[i] == null)
                continue;

            frozenOpponentColors[i] = frozenOpponentRenderers[i].color;
            frozenOpponentRenderers[i].color = freezeTint;
        }

        yield return new WaitForSeconds(freezeDuration);

        freezeRoutine = null;
        ClearFrozenOpponentState();
    }

    Transform FindOpponentRoot()
    {
        Rigidbody2D[] rigidbodies = FindObjectsOfType<Rigidbody2D>(true);
        Transform bestHead = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody2D body = rigidbodies[i];

            if (body == null || body.transform == null || body.transform.name != "Head")
                continue;

            if (playerRb != null && body == playerRb)
                continue;

            float distance = Mathf.Abs(body.transform.position.x - transform.position.x);
            if (distance < 0.1f)
                continue;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestHead = body.transform;
            }
        }

        return bestHead != null ? bestHead.root : null;
    }

    void RestoreFrozenOpponent()
    {
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        ClearFrozenOpponentState();
    }

    void ClearFrozenOpponentState()
    {
        freezeRoutine = null;

        if (frozenOpponentMovement != null)
            frozenOpponentMovement.enabled = frozenOpponentMovementWasEnabled;

        if (frozenOpponentKick != null)
            frozenOpponentKick.enabled = frozenOpponentKickWasEnabled;

        if (frozenOpponentAI != null)
            frozenOpponentAI.enabled = frozenOpponentAIWasEnabled;

        if (frozenOpponentRb != null)
        {
            frozenOpponentRb.linearVelocity = Vector2.zero;
            frozenOpponentRb.angularVelocity = 0f;
        }

        if (frozenOpponentRenderers != null && frozenOpponentColors != null)
        {
            int colorCount = Mathf.Min(frozenOpponentRenderers.Length, frozenOpponentColors.Length);
            for (int i = 0; i < colorCount; i++)
            {
                if (frozenOpponentRenderers[i] != null)
                    frozenOpponentRenderers[i].color = frozenOpponentColors[i];
            }
        }

        frozenOpponentRoot = null;
        frozenOpponentRb = null;
        frozenOpponentMovement = null;
        frozenOpponentKick = null;
        frozenOpponentAI = null;
        frozenOpponentRenderers = null;
        frozenOpponentColors = null;
        frozenOpponentMovementWasEnabled = false;
        frozenOpponentKickWasEnabled = false;
        frozenOpponentAIWasEnabled = false;
    }

    void TriggerStickyBall(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        if (ballRb == null)
            return;

        if (stickyBallRoutine != null)
            StopCoroutine(stickyBallRoutine);

        RestoreStickyBall();
        stickyBallRoutine = StartCoroutine(ApplyStickyBall(ballRb, shotDirection));
    }

    IEnumerator ApplyStickyBall(Rigidbody2D ballRb, Vector2 shotDirection)
    {
        stickyBallRb = ballRb;
        stickyBallSavedGravity = ballRb.gravityScale;
        stickyBallColliders = ballRb.GetComponentsInChildren<Collider2D>(true);
        stickyOwnerColliders = GetComponentsInChildren<Collider2D>(true);

        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.gravityScale = 0f;
        SetStickyBallOwnerCollisionIgnored(true);

        float elapsed = 0f;

        while (elapsed < stickyBallDuration && stickyBallRb != null)
        {
            Transform followTarget = playerRb != null ? playerRb.transform : transform;
            Vector2 holdOffset = new Vector2(
                stickyBallHoldOffset.x * shotDirection.x,
                stickyBallHoldOffset.y
            );

            stickyBallRb.position = (Vector2)followTarget.position + holdOffset;
            stickyBallRb.linearVelocity = Vector2.zero;
            stickyBallRb.angularVelocity = 0f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (stickyBallRb == null)
        {
            stickyBallRoutine = null;
            yield break;
        }

        SetStickyBallOwnerCollisionIgnored(false);
        stickyBallRb.gravityScale = stickyBallSavedGravity;
        stickyBallRb.linearVelocity = Vector2.zero;
        stickyBallRb.angularVelocity = 0f;
        stickyBallRb.AddForce(
            new Vector2(shotDirection.x * stickyBallReleaseHorizontalForce, stickyBallReleaseVerticalForce),
            ForceMode2D.Impulse
        );

        stickyBallRb = null;
        stickyBallRoutine = null;
    }

    void RestoreStickyBall()
    {
        if (stickyBallRoutine != null)
        {
            StopCoroutine(stickyBallRoutine);
            stickyBallRoutine = null;
        }

        if (stickyBallRb != null)
        {
            SetStickyBallOwnerCollisionIgnored(false);
            stickyBallRb.gravityScale = stickyBallSavedGravity;
            stickyBallRb = null;
        }

        stickyBallColliders = null;
        stickyOwnerColliders = null;
    }

    void SetStickyBallOwnerCollisionIgnored(bool ignored)
    {
        if (stickyBallColliders == null || stickyOwnerColliders == null)
            return;

        for (int i = 0; i < stickyBallColliders.Length; i++)
        {
            Collider2D ballCollider = stickyBallColliders[i];
            if (ballCollider == null)
                continue;

            for (int j = 0; j < stickyOwnerColliders.Length; j++)
            {
                Collider2D ownerCollider = stickyOwnerColliders[j];
                if (ownerCollider == null)
                    continue;

                Physics2D.IgnoreCollision(ballCollider, ownerCollider, ignored);
            }
        }
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
        RestoreStickyBall();
        RestoreFrozenOpponent();
        SetSpecialArmed(false);
    }
}
