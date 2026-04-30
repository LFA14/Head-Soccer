using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentResultSequenceUI : MonoBehaviour
{
    [Header("UI References")]
    public Image resultIcon;
    public GameObject coinsGroup;
    public Image coinIcon;
    public TMP_Text coinsText;
    public Image messageIcon;
    public GameObject continueButton;
    public ParticleSystem winConfetti;

    [Header("Sprites")]
    public Sprite winSprite;
    public Sprite lossSprite;
    public Sprite congratsSprite;
    public Sprite hardLuckSprite;
    public Sprite tournamentWonSprite;

    [Header("Audio")]
    public AudioClip winSound;
    public AudioClip lossSound;
    [Range(0f, 1f)] public float resultSoundVolume = 1f;
    [Range(0f, 1f)] public float resultMusicVolumeMultiplier = 0.18f;

    [Header("Timing")]
    public float firstDelay = 0.4f;
    public float betweenDelay = 0.35f;
    public float popDuration = 0.35f;
    public float coinsCountDuration = 1.2f;

    [Header("Scale")]
    public float startScale = 0.6f;
    public float endScale = 1f;
    public float overshootScale = 1.15f;

    readonly List<ParticleSystem> confettiSystems = new List<ParticleSystem>();
    readonly Dictionary<AudioSource, float> duckedAudioVolumes = new Dictionary<AudioSource, float>();
    private AudioSource resultAudioSource;
    private Coroutine restoreMusicRoutine;
    private Button continueButtonComponent;
    private TournamentResultContinueButton continueHandler;
    private bool continueRequested;

    private void Start()
    {
        WireContinueButton();
        SetupConfettiSystems();
        PrepareUI();
        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (continueRequested || continueButton == null || !continueButton.activeInHierarchy)
            return;

        if (WasContinueButtonPressed())
            HandleContinueButtonClicked();
    }

    void WireContinueButton()
    {
        if (continueButton == null)
            return;

        continueButtonComponent = continueButton.GetComponent<Button>();

        if (continueButtonComponent == null)
            return;

        continueHandler = FindObjectOfType<TournamentResultContinueButton>();
        continueButtonComponent.onClick.RemoveListener(HandleContinueButtonClicked);
        continueButtonComponent.onClick.AddListener(HandleContinueButtonClicked);
    }

    void PrepareUI()
    {
        if (resultIcon != null)
        {
            resultIcon.gameObject.SetActive(false);
            resultIcon.transform.localScale = Vector3.one * startScale;
            SetImageAlpha(resultIcon, 0f);
        }

        if (coinsGroup != null)
        {
            coinsGroup.SetActive(false);
            coinsGroup.transform.localScale = Vector3.one * startScale;
        }

        if (coinIcon != null)
            SetImageAlpha(coinIcon, 0f);

        if (coinsText != null)
        {
            coinsText.text = "0";
            SetTextAlpha(coinsText, 0f);
        }

        if (messageIcon != null)
        {
            messageIcon.gameObject.SetActive(false);
            messageIcon.transform.localScale = Vector3.one * startScale;
            SetImageAlpha(messageIcon, 0f);
        }

        if (continueButton != null)
            continueButton.SetActive(false);

        StopConfettiSystems();
    }

    IEnumerator PlaySequence()
    {
        if (TournamentResultData.Instance == null)
        {
            ShowContinueButton();

            yield break;
        }

        var data = TournamentResultData.Instance;

        yield return new WaitForSeconds(firstDelay);

        if (resultIcon != null)
        {
            resultIcon.sprite = data.playerWon ? winSprite : lossSprite;
            PlayOutcomeSound();
            yield return StartCoroutine(PopInImage(resultIcon));
        }

        yield return new WaitForSeconds(betweenDelay);

        if (coinsGroup != null)
        {
            coinsGroup.SetActive(true);
            yield return StartCoroutine(PopInCoinsGroup());
            yield return StartCoroutine(CountCoins(data.rewardCoins));
        }

        yield return new WaitForSeconds(betweenDelay);

        if (messageIcon != null)
        {
            messageIcon.sprite = GetMessageSprite(data);
            yield return StartCoroutine(PopInImage(messageIcon));

            if (data.wonTournament)
                PlayConfettiSystems();
        }

        yield return new WaitForSeconds(0.25f);

        ShowContinueButton();
    }

    void ShowContinueButton()
    {
        if (continueButton == null)
            return;

        continueButton.SetActive(true);
        continueButton.transform.SetAsLastSibling();
    }

    void HandleContinueButtonClicked()
    {
        if (continueRequested)
            return;

        continueRequested = true;

        if (continueHandler == null)
            continueHandler = FindObjectOfType<TournamentResultContinueButton>();

        if (continueHandler != null)
        {
            continueHandler.ContinueAfterTournamentResult();
            return;
        }

        MenuButtonAction.SuppressLoadsFor(0.5f);
        SceneManager.LoadScene("MenuScene");
    }

    bool WasContinueButtonPressed()
    {
        if (Input.GetMouseButtonDown(0))
            return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
                return true;
        }

        return false;
    }

    Sprite GetMessageSprite(TournamentResultData data)
    {
        if (data == null)
            return null;

        if (data.wonTournament)
            return tournamentWonSprite;

        if (data.playerWon)
            return congratsSprite;

        return hardLuckSprite;
    }

    IEnumerator PopInImage(Image img)
    {
        img.gameObject.SetActive(true);

        float time = 0f;
        while (time < popDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popDuration);

            float scale = Mathf.Lerp(startScale, overshootScale, t);
            img.transform.localScale = Vector3.one * scale;
            SetImageAlpha(img, t);

            yield return null;
        }

        time = 0f;
        Vector3 from = Vector3.one * overshootScale;
        Vector3 to = Vector3.one * endScale;

        while (time < popDuration * 0.5f)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / (popDuration * 0.5f));
            img.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        img.transform.localScale = Vector3.one * endScale;
        SetImageAlpha(img, 1f);
    }

    IEnumerator PopInCoinsGroup()
    {
        float time = 0f;

        while (time < popDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popDuration);

            float scale = Mathf.Lerp(startScale, overshootScale, t);
            coinsGroup.transform.localScale = Vector3.one * scale;

            if (coinIcon != null)
                SetImageAlpha(coinIcon, t);

            if (coinsText != null)
                SetTextAlpha(coinsText, t);

            yield return null;
        }

        time = 0f;
        Vector3 from = Vector3.one * overshootScale;
        Vector3 to = Vector3.one * endScale;

        while (time < popDuration * 0.5f)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / (popDuration * 0.5f));
            coinsGroup.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        coinsGroup.transform.localScale = Vector3.one * endScale;

        if (coinIcon != null)
            SetImageAlpha(coinIcon, 1f);

        if (coinsText != null)
            SetTextAlpha(coinsText, 1f);
    }

    IEnumerator CountCoins(int targetCoins)
    {
        float time = 0f;

        while (time < coinsCountDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / coinsCountDuration);

            int currentCoins = Mathf.RoundToInt(Mathf.Lerp(0f, targetCoins, t));
            if (coinsText != null)
                coinsText.text = currentCoins.ToString();

            yield return null;
        }

        if (coinsText != null)
            coinsText.text = targetCoins.ToString();
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SetTextAlpha(TMP_Text txt, float alpha)
    {
        Color c = txt.color;
        c.a = alpha;
        txt.color = c;
    }

    void SetupConfettiSystems()
    {
        confettiSystems.Clear();

        if (winConfetti == null)
            return;

        ConfigureConfettiLayer(winConfetti, new Vector3(0f, 4.8f, 0f), 0f, 22f, 2.6f, 6.4f, 8.2f, 0.18f, 0.42f, 150, 175, 0.8f, 0.05f);
        confettiSystems.Add(winConfetti);

        ParticleSystem midLayer = Instantiate(winConfetti, winConfetti.transform.parent);
        midLayer.name = "WinConfettiMid";
        ConfigureConfettiLayer(midLayer, new Vector3(0f, 2.95f, 0f), 0.16f, 19f, 3.1f, 5.4f, 7.1f, 0.12f, 0.3f, 110, 130, 0.5f, 0.12f);
        confettiSystems.Add(midLayer);

        ParticleSystem lowerLayer = Instantiate(winConfetti, winConfetti.transform.parent);
        lowerLayer.name = "WinConfettiLower";
        ConfigureConfettiLayer(lowerLayer, new Vector3(0f, 1.25f, 0f), 0.32f, 16f, 3.3f, 4.8f, 6.2f, 0.1f, 0.24f, 85, 105, 0.28f, 0.3f);
        confettiSystems.Add(lowerLayer);
    }

    void ConfigureConfettiLayer(
        ParticleSystem system,
        Vector3 position,
        float delay,
        float width,
        float height,
        float lifetimeMin,
        float lifetimeMax,
        float sizeMin,
        float sizeMax,
        short burstMin,
        short burstMax,
        float horizontalDrift,
        float whiteAccent)
    {
        if (system == null)
            return;

        system.transform.localPosition = position;
        system.transform.localRotation = Quaternion.identity;
        system.transform.localScale = Vector3.one;

        var main = system.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 5.4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.22f);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = BuildStartColor(whiteAccent);
        main.gravityModifier = 0.04f;
        main.maxParticles = 320;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(delay, burstMin, burstMax) });

        var shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, height, 0.25f);
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;
        shape.randomDirectionAmount = 0.65f;
        shape.randomPositionAmount = 0.6f;

        var velocity = system.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-horizontalDrift, horizontalDrift);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, -0.08f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

        var noise = system.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = 0.22f;
        noise.strengthY = 0.12f;
        noise.strengthZ = 0.06f;
        noise.frequency = 0.28f;
        noise.scrollSpeed = 0.08f;
        noise.damping = true;

        var colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(BuildConfettiGradient(whiteAccent));

        var sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, BuildSizeCurve());

        var renderer = system.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 20;
            renderer.lengthScale = 1f;
            renderer.velocityScale = 0f;
            renderer.cameraVelocityScale = 0f;
            ApplyConfettiMaterialTint(renderer);
        }

        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    ParticleSystem.MinMaxGradient BuildStartColor(float whiteAccent)
    {
        Color richGold = new Color(0.92f, 0.76f, 0.18f, 1f);
        Color brightGold = new Color(1f, 0.87f, 0.28f, 1f);
        Color softWhite = new Color(1f, 0.985f, 0.95f, 1f);
        Color accentColor = Color.Lerp(brightGold, softWhite, Mathf.Clamp01(whiteAccent));

        return new ParticleSystem.MinMaxGradient(richGold, accentColor);
    }

    void ApplyConfettiMaterialTint(ParticleSystemRenderer renderer)
    {
        if (renderer == null)
            return;

        Material material = renderer.material;
        if (material == null)
            return;

        Color neutralTint = Color.white;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", neutralTint);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", neutralTint);

        if (material.HasProperty("_TintColor"))
            material.SetColor("_TintColor", neutralTint);

        renderer.sharedMaterial = material;
    }

    Gradient BuildConfettiGradient(float whiteAccent)
    {
        Gradient gradient = new Gradient();
        Color deepGold = new Color(0.88f, 0.7f, 0.16f);
        Color brightGold = new Color(1f, 0.86f, 0.3f);
        Color softWhite = new Color(1f, 0.985f, 0.96f);
        Color highlight = Color.Lerp(brightGold, softWhite, Mathf.Clamp01(whiteAccent));

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(deepGold, 0f),
                new GradientColorKey(highlight, 0.4f),
                new GradientColorKey(brightGold, 0.72f),
                new GradientColorKey(deepGold, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.94f, 0.84f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }

    AnimationCurve BuildSizeCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.45f, 1.08f),
            new Keyframe(0.78f, 0.96f),
            new Keyframe(1f, 0.8f)
        );
    }

    void StopConfettiSystems()
    {
        foreach (ParticleSystem system in confettiSystems)
        {
            if (system != null)
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void PlayConfettiSystems()
    {
        foreach (ParticleSystem system in confettiSystems)
        {
            if (system != null)
                system.Play();
        }
    }

    void EnsureAudioSource()
    {
        if (resultAudioSource != null)
            return;

        resultAudioSource = GetComponent<AudioSource>();

        if (resultAudioSource == null)
            resultAudioSource = gameObject.AddComponent<AudioSource>();

        resultAudioSource.playOnAwake = false;
        resultAudioSource.loop = false;
        resultAudioSource.spatialBlend = 0f;
    }

    void PlayOutcomeSound()
    {
        if (TournamentResultData.Instance == null)
            return;

        AudioClip clipToPlay = TournamentResultData.Instance.playerWon ? winSound : lossSound;
        if (clipToPlay == null)
            return;

        EnsureAudioSource();

        if (resultAudioSource == null)
            return;

        DuckBackgroundAudio();

        if (restoreMusicRoutine != null)
            StopCoroutine(restoreMusicRoutine);

        restoreMusicRoutine = StartCoroutine(RestoreMusicAfterDelay(clipToPlay.length));

        resultAudioSource.PlayOneShot(clipToPlay, resultSoundVolume);
    }

    IEnumerator RestoreMusicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay));

        RestoreDuckedAudio();

        restoreMusicRoutine = null;
    }

    void OnDisable()
    {
        if (restoreMusicRoutine != null)
            StopCoroutine(restoreMusicRoutine);

        RestoreDuckedAudio();
    }

    void OnDestroy()
    {
        if (continueButtonComponent != null)
            continueButtonComponent.onClick.RemoveListener(HandleContinueButtonClicked);
    }

    void DuckBackgroundAudio()
    {
        RestoreDuckedAudio();

        AudioSource[] audioSources = FindObjectsOfType<AudioSource>(true);
        float clampedMultiplier = Mathf.Clamp01(resultMusicVolumeMultiplier);

        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource source = audioSources[i];

            if (source == null || source == resultAudioSource)
                continue;

            if (!source.enabled || source.mute || !source.gameObject.activeInHierarchy)
                continue;

            duckedAudioVolumes[source] = source.volume;
            source.volume *= clampedMultiplier;
        }
    }

    void RestoreDuckedAudio()
    {
        foreach (KeyValuePair<AudioSource, float> pair in duckedAudioVolumes)
        {
            if (pair.Key != null)
                pair.Key.volume = pair.Value;
        }

        duckedAudioVolumes.Clear();
    }
}
