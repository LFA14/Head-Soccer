using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public static MenuMusic Instance;

    private AudioSource audioSource;

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
    }

    public void ToggleMusic()
    {
        if (audioSource == null) return;
        audioSource.mute = !audioSource.mute;
    }
}
