using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private static readonly string[] DesiredDisplayNames =
    {
        "ArgentinaCharacter",
        "Character",
        "EgyptCharacter",
        "SaudiCharacter"
    };

    private static readonly string[][] CharacterAliases =
    {
        new[] { "argentinacharacter", "argentinacharacter_0", "argentina", "leo" },
        new[] { "character", "character_0", "ronaldinho", "brazil" },
        new[] { "egyptcharacter", "egyptcharacter_0", "egypt", "faroun" },
        new[] { "saudicharacter", "saudicharacter_0", "saudi", "turki" }
    };

    private void Awake()
    {
        AutoAssignReferences();
        NormalizeCharacters();
        HookButtons();
    }

    private void OnEnable()
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
            HookButtonIfNeeded(leftButton, PreviousCharacter);
        }

        if (rightButton != null)
        {
            HookButtonIfNeeded(rightButton, NextCharacter);
        }
    }

    private void HookButtonIfNeeded(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || button.onClick.GetPersistentEventCount() > 0)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
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

    public void OpenCostumizeScene()
    {
        SceneManager.LoadScene("costumize");
    }

    private void ShowCharacter(bool animate)
    {
        if (characters == null || characters.Length == 0)
            return;

        if (currentIndex < 0 || currentIndex >= characters.Length)
            currentIndex = 0;

        CharacterData currentCharacter = characters[currentIndex];
        if (currentCharacter == null)
            return;

        Sprite portraitSprite = currentCharacter.portrait != null ? currentCharacter.portrait : currentCharacter.storyPicture;
        Sprite storySprite = currentCharacter.storyPicture != null ? currentCharacter.storyPicture : currentCharacter.portrait;

        if (playerPortrait != null)
        {
            playerPortrait.sprite = portraitSprite;
            playerPortrait.preserveAspect = true;
            playerPortrait.enabled = portraitSprite != null;

            Color portraitColor = playerPortrait.color;
            portraitColor.a = 1f;
            playerPortrait.color = portraitColor;
        }

        if (storyImage != null)
        {
            storyImage.sprite = storySprite;
            storyImage.preserveAspect = true;
            storyImage.enabled = storySprite != null;

            Color storyColor = storyImage.color;
            storyColor.a = 1f;
            storyImage.color = storyColor;
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

    private void AutoAssignReferences()
    {
        if (playerPortrait == null)
            playerPortrait = FindByName<Image>("PlayerPortrait");

        if (storyImage == null)
            storyImage = FindByName<Image>("StoryImage");

        if (leftButton == null)
            leftButton = FindByName<Button>("PlayerUp");

        if (rightButton == null)
            rightButton = FindByName<Button>("PlayerDown");
    }

    private void NormalizeCharacters()
    {
        if (characters == null || characters.Length == 0)
            return;

        CharacterData[] sourceCharacters = characters;
        List<CharacterData> normalizedCharacters = new List<CharacterData>();
        HashSet<int> usedIndexes = new HashSet<int>();

        for (int i = 0; i < DesiredDisplayNames.Length; i++)
        {
            int matchIndex = FindCharacterIndex(sourceCharacters, CharacterAliases[i], usedIndexes);
            if (matchIndex < 0)
                matchIndex = FindFallbackCharacterIndex(sourceCharacters, i, usedIndexes);

            if (matchIndex < 0)
            {
                normalizedCharacters.Add(new CharacterData
                {
                    characterId = DesiredDisplayNames[i],
                    characterName = DesiredDisplayNames[i]
                });
                continue;
            }

            CharacterData source = sourceCharacters[matchIndex];
            normalizedCharacters.Add(new CharacterData
            {
                characterId = string.IsNullOrWhiteSpace(source.characterId) ? DesiredDisplayNames[i] : source.characterId,
                characterName = DesiredDisplayNames[i],
                portrait = source.portrait,
                storyPicture = source.storyPicture
            });

            usedIndexes.Add(matchIndex);
        }

        if (normalizedCharacters.Count == 0)
            return;

        for (int i = 0; i < sourceCharacters.Length; i++)
        {
            if (usedIndexes.Contains(i) || sourceCharacters[i] == null)
                continue;

            normalizedCharacters.Add(sourceCharacters[i]);
        }

        characters = normalizedCharacters.ToArray();

        if (currentIndex < 0 || currentIndex >= characters.Length)
            currentIndex = 0;
    }

    private int FindCharacterIndex(CharacterData[] sourceCharacters, string[] aliases, HashSet<int> usedIndexes)
    {
        for (int i = 0; i < sourceCharacters.Length; i++)
        {
            if (usedIndexes.Contains(i) || sourceCharacters[i] == null)
                continue;

            if (MatchesAlias(sourceCharacters[i], aliases))
                return i;
        }

        return -1;
    }

    private int FindFallbackCharacterIndex(CharacterData[] sourceCharacters, int desiredIndex, HashSet<int> usedIndexes)
    {
        if (desiredIndex >= 0 && desiredIndex < sourceCharacters.Length && sourceCharacters[desiredIndex] != null && !usedIndexes.Contains(desiredIndex))
            return desiredIndex;

        for (int i = 0; i < sourceCharacters.Length; i++)
        {
            if (sourceCharacters[i] != null && !usedIndexes.Contains(i))
                return i;
        }

        return -1;
    }

    private bool MatchesAlias(CharacterData data, string[] aliases)
    {
        string characterIdValue = NormalizeKey(data.characterId);
        string characterNameValue = NormalizeKey(data.characterName);
        string portraitNameValue = data.portrait != null ? NormalizeKey(data.portrait.name) : string.Empty;
        string storyNameValue = data.storyPicture != null ? NormalizeKey(data.storyPicture.name) : string.Empty;

        for (int i = 0; i < aliases.Length; i++)
        {
            string alias = aliases[i];
            if (characterIdValue == alias || characterNameValue == alias || portraitNameValue == alias || storyNameValue == alias)
                return true;
        }

        return false;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private T FindByName<T>(string objectName) where T : Component
    {
        T[] components = FindObjectsOfType<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null && components[i].name == objectName)
                return components[i];
        }

        return null;
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
