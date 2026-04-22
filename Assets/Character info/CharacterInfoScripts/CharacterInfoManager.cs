using System.Collections;
using System.Collections.Generic;
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

    private static readonly string[] DesiredDisplayNames =
    {
        "ArgentinaCharacter",
        "Character",
        "EgyptCharacter",
        "SaudiCharacter"
    };

    private static readonly string[][] CharacterAliases =
    {
        new[] { "argentinacharacter", "argentina", "leo" },
        new[] { "character", "ronaldinho", "brazil" },
        new[] { "egyptcharacter", "egypt", "faroun" },
        new[] { "saudicharacter", "saudi", "turki" }
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

        if (currentIndex < 0 || currentIndex >= characters.Length)
            currentIndex = 0;

        CharacterData currentCharacter = characters[currentIndex];

        if (playerPortrait != null)
        {
            playerPortrait.sprite = currentCharacter.portrait;
            playerPortrait.preserveAspect = true;
            playerPortrait.enabled = currentCharacter.portrait != null;

            Color portraitColor = playerPortrait.color;
            portraitColor.a = 1f;
            playerPortrait.color = portraitColor;
        }

        if (storyImage != null)
        {
            storyImage.sprite = currentCharacter.storyPicture;
            storyImage.preserveAspect = true;
            storyImage.enabled = currentCharacter.storyPicture != null;

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

        List<CharacterData> normalizedCharacters = new List<CharacterData>();
        HashSet<int> usedIndexes = new HashSet<int>();

        for (int i = 0; i < DesiredDisplayNames.Length; i++)
        {
            int matchIndex = FindCharacterIndex(CharacterAliases[i], usedIndexes);
            if (matchIndex < 0)
                continue;

            CharacterData source = characters[matchIndex];
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

        for (int i = 0; i < characters.Length; i++)
        {
            if (usedIndexes.Contains(i) || characters[i] == null)
                continue;

            normalizedCharacters.Add(characters[i]);
        }

        characters = normalizedCharacters.ToArray();

        if (currentIndex < 0 || currentIndex >= characters.Length)
            currentIndex = 0;
    }

    private int FindCharacterIndex(string[] aliases, HashSet<int> usedIndexes)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (usedIndexes.Contains(i) || characters[i] == null)
                continue;

            if (MatchesAlias(characters[i], aliases))
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
