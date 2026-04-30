using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusic : MonoBehaviour
{
    public static MenuMusic Instance;

    private static readonly string[] DefaultAllowedSceneNames =
    {
        "MenuScene",
        "CharacterSelectScene",
        "TournamentScene",
        "OnlineLobbyScene"
    };

    [SerializeField] private string[] allowedSceneNames =
    {
        "MenuScene",
        "CharacterSelectScene",
        "TournamentScene",
        "OnlineLobbyScene"
    };

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneMusicState(SceneManager.GetActiveScene().name);
    }

    public void ToggleMusic()
    {
        if (audioSource == null)
            return;

        userMuted = !userMuted;
        ApplyMuteState();

        if (!userMuted && IsMusicAllowedInScene(SceneManager.GetActiveScene().name) && !audioSource.isPlaying)
            audioSource.Play();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneMusicState(scene.name);
    }

    private void ApplySceneMusicState(string sceneName)
    {
        if (audioSource == null)
            return;

        if (IsMusicAllowedInScene(sceneName))
        {
            gameplayMuted = false;
            RestoreOriginalVolume();
            ApplyMuteState();

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            gameplayMuted = false;
            audioSource.Stop();
            RestoreOriginalVolume();
            ApplyMuteState();
        }
    }

    private bool IsMusicAllowedInScene(string sceneName)
    {
        string[] sceneNames = allowedSceneNames != null && allowedSceneNames.Length > 0
            ? allowedSceneNames
            : DefaultAllowedSceneNames;

        for (int i = 0; i < sceneNames.Length; i++)
        {
            if (sceneNames[i] == sceneName)
                return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
