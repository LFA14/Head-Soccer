using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CostumizeSceneController : MonoBehaviour
{
    private readonly string[] prefabNames =
    {
        "ArgentinaPlayer",
        "BrazilPlayer",
        "EgyptCharacter",
        "SaudiCharacter"
    };

    private readonly string[] displayNames =
    {
        "ARGENTINA",
        "BRAZIL",
        "EGYPT",
        "SAUDI"
    };

    private int characterIndex;
    private GameObject previewCharacter;
    private TMP_Text characterNameText;
    private Image shoeSwatch;
    private Image tieSwatch;

    private void Start()
    {
        BuildInterface();
        ShowCharacter();
    }

    public void PreviousCharacter()
    {
        characterIndex = (characterIndex - 1 + PlayerCustomizationSave.CharacterCount) % PlayerCustomizationSave.CharacterCount;
        ShowCharacter();
    }

    public void NextCharacter()
    {
        characterIndex = (characterIndex + 1) % PlayerCustomizationSave.CharacterCount;
        ShowCharacter();
    }

    public void PreviousShoeColor()
    {
        string id = CurrentCharacterId();
        PlayerCustomizationSave.SetShoeColorIndex(id, PlayerCustomizationSave.GetShoeColorIndex(id) - 1);
        RefreshColors();
    }

    public void NextShoeColor()
    {
        string id = CurrentCharacterId();
        PlayerCustomizationSave.SetShoeColorIndex(id, PlayerCustomizationSave.GetShoeColorIndex(id) + 1);
        RefreshColors();
    }

    public void PreviousTieColor()
    {
        string id = CurrentCharacterId();
        PlayerCustomizationSave.SetTieColorIndex(id, PlayerCustomizationSave.GetTieColorIndex(id) - 1);
        RefreshColors();
    }

    public void NextTieColor()
    {
        string id = CurrentCharacterId();
        PlayerCustomizationSave.SetTieColorIndex(id, PlayerCustomizationSave.GetTieColorIndex(id) + 1);
        RefreshColors();
    }

    public void BackToUpgrades()
    {
        SceneManager.LoadScene("CharactersInfoScene");
    }

    private void ShowCharacter()
    {
        if (previewCharacter != null)
            Destroy(previewCharacter);

        GameObject prefab = Resources.Load<GameObject>("Characters/" + prefabNames[characterIndex]);
        if (prefab != null)
        {
            previewCharacter = Instantiate(prefab, new Vector3(0f, -1.35f, 0f), Quaternion.identity);
            previewCharacter.transform.localScale = Vector3.one * 1.35f;
            DisablePreviewBehaviour(previewCharacter);
        }

        if (characterNameText != null)
            characterNameText.text = displayNames[characterIndex];

        RefreshColors();
    }

    private void RefreshColors()
    {
        string id = CurrentCharacterId();

        if (previewCharacter != null)
            PlayerCustomizationApplier.ApplyToPlayer(previewCharacter, id);

        if (shoeSwatch != null)
            shoeSwatch.color = PlayerCustomizationSave.GetShoeColor(id);

        if (tieSwatch != null)
            tieSwatch.color = PlayerCustomizationSave.GetTieColor(id);
    }

    private string CurrentCharacterId()
    {
        return PlayerCustomizationSave.GetCharacterId(characterIndex);
    }

    private void DisablePreviewBehaviour(GameObject root)
    {
        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = false;
        }

        Rigidbody2D[] rigidbodies = root.GetComponentsInChildren<Rigidbody2D>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].linearVelocity = Vector2.zero;
            rigidbodies[i].angularVelocity = 0f;
            rigidbodies[i].bodyType = RigidbodyType2D.Kinematic;
            rigidbodies[i].simulated = false;
        }

        Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    private void BuildInterface()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        EnsureEventSystem();

        Image shade = CreatePanel(canvas.transform, "ContentShade", new Vector2(0f, 0f), new Vector2(1920f, 1080f), new Color(0f, 0f, 0f, 0.28f));
        shade.raycastTarget = false;
        shade.transform.SetAsLastSibling();

        characterNameText = CreateText(canvas.transform, "CharacterName", displayNames[0], new Vector2(0f, 405f), new Vector2(720f, 90f), 68f);

        CreateButton(canvas.transform, "BackButton", "BACK", new Vector2(-780f, 430f), new Vector2(240f, 82f), BackToUpgrades);
        CreateButton(canvas.transform, "PreviousCharacter", "<", new Vector2(-540f, 50f), new Vector2(130f, 130f), PreviousCharacter);
        CreateButton(canvas.transform, "NextCharacter", ">", new Vector2(540f, 50f), new Vector2(130f, 130f), NextCharacter);

        CreateText(canvas.transform, "ShoesLabel", "SHOES", new Vector2(-360f, -275f), new Vector2(260f, 68f), 46f);
        CreateButton(canvas.transform, "PreviousShoeColor", "<", new Vector2(-540f, -370f), new Vector2(110f, 88f), PreviousShoeColor);
        shoeSwatch = CreatePanel(canvas.transform, "ShoeColor", new Vector2(-360f, -370f), new Vector2(130f, 88f), Color.white);
        CreateButton(canvas.transform, "NextShoeColor", ">", new Vector2(-180f, -370f), new Vector2(110f, 88f), NextShoeColor);

        CreateText(canvas.transform, "TieLabel", "TIE", new Vector2(360f, -275f), new Vector2(260f, 68f), 46f);
        CreateButton(canvas.transform, "PreviousTieColor", "<", new Vector2(180f, -370f), new Vector2(110f, 88f), PreviousTieColor);
        tieSwatch = CreatePanel(canvas.transform, "TieColor", new Vector2(360f, -370f), new Vector2(130f, 88f), Color.white);
        CreateButton(canvas.transform, "NextTieColor", ">", new Vector2(540f, -370f), new Vector2(110f, 88f), NextTieColor);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private Image CreatePanel(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panelObject = new GameObject(objectName);
        panelObject.transform.SetParent(parent, false);
        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private TMP_Text CreateText(Transform parent, string objectName, string value, Vector2 anchoredPosition, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.color = Color.white;
        return text;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        Image image = CreatePanel(parent, objectName, anchoredPosition, size, new Color(0.88f, 0.18f, 0.08f, 0.95f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        TMP_Text text = CreateText(image.transform, "Text", label, Vector2.zero, size, label.Length > 1 ? 38f : 66f);
        text.raycastTarget = false;
        return button;
    }
}
