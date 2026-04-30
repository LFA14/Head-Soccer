using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonAction : MonoBehaviour
{
    private static float suppressLoadsUntilRealtime;

    public AudioSource sfxSource;
    public AudioClip clickSfx;
    public float clickDelay = 0.15f;

    public static void SuppressLoadsFor(float seconds)
    {
        suppressLoadsUntilRealtime = Mathf.Max(suppressLoadsUntilRealtime, Time.unscaledTime + seconds);
    }

    public void LoadScene(string sceneName)
    {
        if (Time.unscaledTime < suppressLoadsUntilRealtime)
            return;

        if (sfxSource != null && clickSfx != null)
            sfxSource.PlayOneShot(clickSfx);

        StartCoroutine(LoadAfterClick(sceneName));
    }

    private IEnumerator LoadAfterClick(string sceneName)
    {
        yield return new WaitForSecondsRealtime(clickDelay);
        SceneManager.LoadScene(sceneName);
    }
    private void Awake()
{
    DontDestroyOnLoad(gameObject);
}
}
