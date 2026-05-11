using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CostumizeSceneController : MonoBehaviour
{
    private const float PreviewScale = 2.15f;
    private static readonly Vector2 PreviewCenterOffset = new Vector2(0f, 0f);
    private static readonly Vector2 BackButtonSize = new Vector2(1200f, 420f);
    private static readonly Vector2 MainArrowButtonSize = new Vector2(520f, 520f);
    private static readonly Vector2 SmallArrowButtonSize = new Vector2(220f, 180f);

    [Header("Button Art")]
    public Sprite backButtonSprite;
    public Sprite arrowButtonSprite;

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
            previewCharacter = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            previewCharacter.transform.localScale = Vector3.one * PreviewScale;
            DisablePreviewBehaviour(previewCharacter);
            CenterPreviewCharacter(previewCharacter);
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

    private void CenterPreviewCharacter(GameObject root)
    {
        if (root == null)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            root.transform.position = new Vector3(0f, -0.2f, 0f);
            return;
        }

        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        Bounds? previewBounds = BuildPreviewBounds(renderers);
        if (!previewBounds.HasValue)
        {
            root.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0f);
            return;
        }

        Bounds bounds = previewBounds.Value;

        Vector3 targetCenter = new Vector3(
            mainCamera.transform.position.x + PreviewCenterOffset.x,
            mainCamera.transform.position.y + PreviewCenterOffset.y,
            bounds.center.z);

        root.transform.position += targetCenter - bounds.center;
    }

    private Bounds? BuildPreviewBounds(SpriteRenderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
            return null;

        Bounds bounds = default;
        bool foundRenderer = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer == null || renderer.sprite == null || ShouldIgnorePreviewRenderer(renderer))
                continue;

            if (!foundRenderer)
            {
                bounds = renderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!foundRenderer)
            return null;

        return bounds;
    }

    private bool ShouldIgnorePreviewRenderer(SpriteRenderer renderer)
    {
        string key = renderer.gameObject.name.ToLowerInvariant();
        return key.Contains("fireaura") || key.Contains("aura");
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

        Button backButton = CreateSpriteButton(canvas.transform, "BackButton", new Vector2(40f, -40f), BackButtonSize, BackToUpgrades, backButtonSprite);
        SetAnchors(backButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        CreateSpriteButton(canvas.transform, "PreviousCharacter", new Vector2(-640f, 40f), MainArrowButtonSize, PreviousCharacter, arrowButtonSprite);
        CreateSpriteButton(canvas.transform, "NextCharacter", new Vector2(640f, 40f), MainArrowButtonSize, NextCharacter, arrowButtonSprite, true);

        CreateText(canvas.transform, "ShoesLabel", "SHOES", new Vector2(-360f, -275f), new Vector2(260f, 68f), 46f);
        CreateSpriteButton(canvas.transform, "PreviousShoeColor", new Vector2(-540f, -370f), SmallArrowButtonSize, PreviousShoeColor, arrowButtonSprite);
        shoeSwatch = CreatePanel(canvas.transform, "ShoeColor", new Vector2(-360f, -370f), new Vector2(130f, 88f), Color.white);
        CreateSpriteButton(canvas.transform, "NextShoeColor", new Vector2(-180f, -370f), SmallArrowButtonSize, NextShoeColor, arrowButtonSprite, true);

        CreateText(canvas.transform, "TieLabel", "TIE", new Vector2(360f, -275f), new Vector2(260f, 68f), 46f);
        CreateSpriteButton(canvas.transform, "PreviousTieColor", new Vector2(180f, -370f), SmallArrowButtonSize, PreviousTieColor, arrowButtonSprite);
        tieSwatch = CreatePanel(canvas.transform, "TieColor", new Vector2(360f, -370f), new Vector2(130f, 88f), Color.white);
        CreateSpriteButton(canvas.transform, "NextTieColor", new Vector2(540f, -370f), SmallArrowButtonSize, NextTieColor, arrowButtonSprite, true);
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

    private Button CreateSpriteButton(
        Transform parent,
        string objectName,
        Vector2 anchoredPosition,
        Vector2 size,
        UnityEngine.Events.UnityAction action,
        Sprite sprite,
        bool flipHorizontally = false)
    {
        if (sprite == null)
            return CreateButton(parent, objectName, objectName.Contains("Back") ? "BACK" : ">", anchoredPosition, size, action);

        Image image = CreatePanel(parent, objectName, anchoredPosition, size, Color.white);
        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.localScale = new Vector3(flipHorizontally ? -1f : 1f, 1f, 1f);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        return button;
    }

    private void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
    }
}
