using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public static MenuMusic Instance;

    private AudioSource audioSource;
    private float originalVolume = 1f;
    private bool userMuted;
    private bool gameplayMuted;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            originalVolume = audioSource.volume;
    }

    public void ToggleMusic()
    {
        if (audioSource == null)
            return;

        userMuted = !userMuted;
        ApplyMuteState();
    }

    public void SetGameplayMuted(bool muted)
    {
        if (audioSource == null)
            return;

        gameplayMuted = muted;
        ApplyMuteState();
    }

    public void SetVolumeMultiplier(float multiplier)
    {
        if (audioSource == null)
            return;

        audioSource.volume = originalVolume * Mathf.Clamp01(multiplier);
    }

    public void RestoreOriginalVolume()
    {
        if (audioSource == null)
            return;

        audioSource.volume = originalVolume;
    }

    private void ApplyMuteState()
    {
        audioSource.mute = userMuted || gameplayMuted;
    }
}
