using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterId;
        public string characterName;
        public Sprite portrait;
        public Sprite storyPicture;
    }

    [Header("Character Data")]
    public CharacterData[] characters;

    [Header("UI")]
    public Image playerPortrait;
    public Image storyImage;
    public TMP_Text playerNameText;

    [Header("Upgrade Panels")]
    public StatUpgradePanelUI speedPanel;
    public StatUpgradePanelUI jumpPanel;
    public StatUpgradePanelUI kickPanel;

    [Header("Arrow Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Animation")]
    public float popScale = 1.12f;
    public float popDuration = 0.12f;

    private int currentIndex = 0;
    private Coroutine portraitPopRoutine;
    private Coroutine storyPopRoutine;
    private Coroutine namePopRoutine;

    private void Awake()
    {
        HookButtons();
    }

    private void Start()
    {
        ShowCharacter(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextCharacter();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousCharacter();
    }

    private void HookButtons()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(PreviousCharacter);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(NextCharacter);
        }
    }

    public void NextCharacter()
    {
        if (characters == null || characters.Length == 0)
            return;

        currentIndex = (currentIndex + 1) % characters.Length;
        ShowCharacter(true);
    }

    public void PreviousCharacter()
    {
        if (characters == null || characters.Length == 0)
            return;

        currentIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        ShowCharacter(true);
    }

    private void ShowCharacter(bool animate)
    {
        if (characters == null || characters.Length == 0)
            return;

        CharacterData currentCharacter = characters[currentIndex];

        if (playerPortrait != null)
        {
            playerPortrait.sprite = currentCharacter.portrait;
            playerPortrait.preserveAspect = true;
        }

        if (storyImage != null)
        {
            storyImage.sprite = currentCharacter.storyPicture;
            storyImage.preserveAspect = true;
        }

        if (playerNameText != null)
            playerNameText.text = currentCharacter.characterName;

        UpdateUpgradePanels();

        if (animate)
        {
            if (playerPortrait != null)
            {
                if (portraitPopRoutine != null)
                    StopCoroutine(portraitPopRoutine);

                portraitPopRoutine = StartCoroutine(Pop(playerPortrait.rectTransform));
            }

            if (storyImage != null)
            {
                if (storyPopRoutine != null)
                    StopCoroutine(storyPopRoutine);

                storyPopRoutine = StartCoroutine(Pop(storyImage.rectTransform));
            }

            if (playerNameText != null)
            {
                if (namePopRoutine != null)
                    StopCoroutine(namePopRoutine);

                namePopRoutine = StartCoroutine(Pop(playerNameText.rectTransform));
            }
        }
    }

    private void UpdateUpgradePanels()
    {
        if (characters == null || characters.Length == 0)
            return;

        string currentCharacterId = characters[currentIndex].characterId;

        if (speedPanel != null)
            speedPanel.Setup(currentCharacterId, "speed");

        if (jumpPanel != null)
            jumpPanel.Setup(currentCharacterId, "jump");

        if (kickPanel != null)
            kickPanel.Setup(currentCharacterId, "shot");
    }

    private IEnumerator Pop(RectTransform target)
    {
        if (target == null)
            yield break;

        Vector3 startScale = Vector3.one;
        Vector3 peakScale = Vector3.one * popScale;

        float halfDuration = popDuration * 0.5f;
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            target.localScale = Vector3.Lerp(startScale, peakScale, t);
            yield return null;
        }

        time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            target.localScale = Vector3.Lerp(peakScale, startScale, t);
            yield return null;
        }

        target.localScale = startScale;
    }
}