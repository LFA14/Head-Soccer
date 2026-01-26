using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonAction : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip clickSfx;
    public float clickDelay = 0.15f;

    public void LoadScene(string sceneName)
    {
        if (sfxSource != null && clickSfx != null)
            sfxSource.PlayOneShot(clickSfx);

        StartCoroutine(LoadAfterClick(sceneName));
    }

    private IEnumerator LoadAfterClick(string sceneName)
    {
        yield return new WaitForSecondsRealtime(clickDelay);
        SceneManager.LoadScene(sceneName);
    }
}
