using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string menuSceneName = "MenuScene";

    private bool isPaused;

    private void Reset()
    {
        CacheReferencesIfNeeded();
    }

    private void Awake()
    {
        CacheReferencesIfNeeded();

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        CacheReferencesIfNeeded();

        if (isPaused || pausePanel == null)
            return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        SelectResumeButton();
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;
        ClearSelectedButton();
    }

    public void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void CacheReferencesIfNeeded()
    {
        if (pausePanel == null)
            pausePanel = FindSceneObjectByName("PausePanel");

        if (resumeButton == null)
            resumeButton = FindButtonByName("ResumeButton");

        if (quitButton == null)
            quitButton = FindButtonByName("QuitButton");
    }

    private Button FindButtonByName(string buttonName)
    {
        if (pausePanel != null)
        {
            Transform[] children = pausePanel.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == buttonName)
                    return children[i].GetComponent<Button>();
            }
        }

        GameObject foundObject = FindSceneObjectByName(buttonName);
        return foundObject != null ? foundObject.GetComponent<Button>() : null;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform currentTransform = allTransforms[i];

            if (currentTransform.name != objectName)
                continue;

            if (!currentTransform.gameObject.scene.IsValid())
                continue;

            return currentTransform.gameObject;
        }

        return null;
    }

    private void SelectResumeButton()
    {
        if (resumeButton == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
    }
}
