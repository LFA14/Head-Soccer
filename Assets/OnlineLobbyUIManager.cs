using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OnlineLobbyUIManager : MonoBehaviour
{
    [Header("Character Order")]
    [SerializeField] private Sprite[] portraitSprites;
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("Fake Lobby Timing")]
    [SerializeField] private bool simulateOpponentJoinOnCreate = true;
    [SerializeField] private bool simulateOpponentAutoReady = true;
    [SerializeField] private float fakeOpponentJoinDelay = 1.75f;
    [SerializeField] private float fakeOpponentReadyDelay = 0.75f;

    [Header("UI Colors")]
    [SerializeField] private Color readyButtonNormalTint = Color.white;
    [SerializeField] private Color readyButtonReadyTint = new Color(0.75f, 1f, 0.75f, 1f);
    [SerializeField] private Color joinPromptNormalColor = Color.white;
    [SerializeField] private Color joinPromptErrorColor = new Color(1f, 0.4f, 0.4f, 1f);

    [Header("Fallbacks")]
    [SerializeField] private string fallbackMainMenuSceneName = "MenuScene";
    [SerializeField] private string emptyLobbyCodeDisplay = "------";

    private GameObject startPanel;
    private GameObject joinPanel;
    private GameObject roomPanel;

    private Button createLobbyButton;
    private Button joinLobbyButton;
    private Button backBtn;
    private Button confirmJoinButton;
    private Button joinBackButton;
    private Button readyButton;
    private Button leaveLobbyButton;
    private Button playerUpButton;
    private Button playerDownButton;

    private TMP_Text joinTitleText;
    private TMP_InputField lobbyCodeInput;
    private TMP_Text lobbyCodeText;
    private TMP_Text statusText;

    private Image player1PortraitImage;
    private Image player2PortraitImage;
    private Image readyButtonImage;

    private ReturnToMenuButton returnToMenuButton;

    private readonly System.Random random = new System.Random();

    private FakeLobbyManager fakeLobbyManager;
    private LobbyCharacterSelectManager characterSelectManager;

    private Coroutine fakeOpponentJoinRoutine;
    private Coroutine fakeOpponentReadyRoutine;

    private string defaultJoinPromptText = "Enter Lobby Code";
    private string defaultStatusText = FakeLobbyManager.NotReadyStatus;
    private string defaultLobbyCodeText = "------";
    private bool listenersRegistered;

    public int SelectedCharacterIndex => characterSelectManager != null ? characterSelectManager.LocalCharacterIndex : 0;
    public GameObject SelectedCharacterPrefab => characterSelectManager != null ? characterSelectManager.GetLocalPrefab() : null;
    public GameObject OpponentCharacterPrefab => characterSelectManager != null ? characterSelectManager.GetOpponentPrefab() : null;

    private void Awake()
    {
        AutoAssignReferences();
        CacheDefaultTextValues();

        fakeLobbyManager = new FakeLobbyManager();
        characterSelectManager = new LobbyCharacterSelectManager(portraitSprites, characterPrefabs);

        RegisterListeners();
        ApplyPlayer1Portrait();
        ClearPlayer2Portrait();
    }

    private void Start()
    {
        ResetToStartPanel();
    }

    private void OnDestroy()
    {
        UnregisterListeners();
        StopLobbySimulation();
    }

    private void RegisterListeners()
    {
        if (listenersRegistered)
        {
            return;
        }

        AddButtonListener(createLobbyButton, OnCreateLobbyPressed);
        AddButtonListener(joinLobbyButton, OnJoinLobbyPressed);
        AddButtonListener(backBtn, ReturnToMainMenu);
        AddButtonListener(confirmJoinButton, OnConfirmJoinPressed);
        AddButtonListener(joinBackButton, ResetToStartPanel);
        AddButtonListener(readyButton, OnReadyPressed);
        AddButtonListener(leaveLobbyButton, OnLeaveLobbyPressed);
        AddButtonListener(playerUpButton, OnPlayerUpPressed);
        AddButtonListener(playerDownButton, OnPlayerDownPressed);

        if (lobbyCodeInput != null)
        {
            lobbyCodeInput.onSubmit.AddListener(OnLobbyCodeSubmitted);
        }

        listenersRegistered = true;
    }

    private void UnregisterListeners()
    {
        if (!listenersRegistered)
        {
            return;
        }

        RemoveButtonListener(createLobbyButton, OnCreateLobbyPressed);
        RemoveButtonListener(joinLobbyButton, OnJoinLobbyPressed);
        RemoveButtonListener(backBtn, ReturnToMainMenu);
        RemoveButtonListener(confirmJoinButton, OnConfirmJoinPressed);
        RemoveButtonListener(joinBackButton, ResetToStartPanel);
        RemoveButtonListener(readyButton, OnReadyPressed);
        RemoveButtonListener(leaveLobbyButton, OnLeaveLobbyPressed);
        RemoveButtonListener(playerUpButton, OnPlayerUpPressed);
        RemoveButtonListener(playerDownButton, OnPlayerDownPressed);

        if (lobbyCodeInput != null)
        {
            lobbyCodeInput.onSubmit.RemoveListener(OnLobbyCodeSubmitted);
        }

        listenersRegistered = false;
    }

    private void OnCreateLobbyPressed()
    {
        StopLobbySimulation();

        fakeLobbyManager.CreateLobby();
        characterSelectManager.ClearOpponent();

        ShowPanels(showStart: false, showJoin: false, showRoom: true);
        ResetJoinPrompt();
        ClearLobbyCodeInput();
        ApplyPlayer1Portrait();
        ClearPlayer2Portrait();
        SetLobbyCode(fakeLobbyManager.LobbyCode);
        RefreshRoomStatus();

        if (simulateOpponentJoinOnCreate)
        {
            fakeOpponentJoinRoutine = StartCoroutine(SimulateOpponentJoinAfterDelay());
        }
    }

    private void OnJoinLobbyPressed()
    {
        StopLobbySimulation();
        ResetJoinPrompt();
        ShowPanels(showStart: false, showJoin: true, showRoom: false);
    }

    private void OnConfirmJoinPressed()
    {
        string normalizedCode = FakeLobbyManager.NormalizeLobbyCode(lobbyCodeInput != null ? lobbyCodeInput.text : string.Empty);
        if (string.IsNullOrEmpty(normalizedCode))
        {
            SetJoinPrompt("Please enter a lobby code.", true);
            return;
        }

        StopLobbySimulation();

        fakeLobbyManager.JoinLobby(normalizedCode);
        characterSelectManager.PickOpponentIndex(random);
        fakeLobbyManager.SetOpponentJoined(characterSelectManager.OpponentCharacterIndex);

        ShowPanels(showStart: false, showJoin: false, showRoom: true);
        ResetJoinPrompt();
        ApplyPlayer1Portrait();
        ApplyPlayer2Portrait();
        SetLobbyCode(fakeLobbyManager.LobbyCode);
        UpdateStatusText(FakeLobbyManager.ConnectedStatus);
        UpdateReadyButtonVisual();

        if (simulateOpponentAutoReady)
        {
            fakeOpponentReadyRoutine = StartCoroutine(SimulateOpponentReadyAfterDelay());
        }
    }

    private void OnReadyPressed()
    {
        if (!fakeLobbyManager.InRoom)
        {
            return;
        }

        fakeLobbyManager.ToggleLocalReady();
        RefreshRoomStatus();

        if (simulateOpponentAutoReady && fakeLobbyManager.OpponentPresent && !fakeLobbyManager.OpponentReady && fakeOpponentReadyRoutine == null)
        {
            fakeOpponentReadyRoutine = StartCoroutine(SimulateOpponentReadyAfterDelay());
        }
    }

    private void OnLeaveLobbyPressed()
    {
        ResetToStartPanel();
    }

    private void OnPlayerUpPressed()
    {
        characterSelectManager.MovePrevious();
        ApplyPlayer1Portrait();
        KeepOpponentDifferentIfNeeded();
    }

    private void OnPlayerDownPressed()
    {
        characterSelectManager.MoveNext();
        ApplyPlayer1Portrait();
        KeepOpponentDifferentIfNeeded();
    }

    private void OnLobbyCodeSubmitted(string _)
    {
        if (joinPanel != null && joinPanel.activeSelf)
        {
            OnConfirmJoinPressed();
        }
    }

    private void ResetToStartPanel()
    {
        StopLobbySimulation();
        fakeLobbyManager.Reset();
        characterSelectManager.ClearOpponent();

        ShowPanels(showStart: true, showJoin: false, showRoom: false);
        ResetJoinPrompt();
        ClearLobbyCodeInput();
        ClearPlayer2Portrait();
        ApplyPlayer1Portrait();
        SetLobbyCode(defaultLobbyCodeText);
        UpdateStatusText(defaultStatusText);
        UpdateReadyButtonVisual();
    }

    private IEnumerator SimulateOpponentJoinAfterDelay()
    {
        yield return new WaitForSeconds(fakeOpponentJoinDelay);

        fakeOpponentJoinRoutine = null;

        if (!fakeLobbyManager.InRoom || !fakeLobbyManager.IsHost)
        {
            yield break;
        }

        characterSelectManager.PickOpponentIndex(random);
        fakeLobbyManager.SetOpponentJoined(characterSelectManager.OpponentCharacterIndex);
        ApplyPlayer2Portrait();
        RefreshRoomStatus();

        if (simulateOpponentAutoReady)
        {
            fakeOpponentReadyRoutine = StartCoroutine(SimulateOpponentReadyAfterDelay());
        }
    }

    private IEnumerator SimulateOpponentReadyAfterDelay()
    {
        yield return new WaitForSeconds(fakeOpponentReadyDelay);

        fakeOpponentReadyRoutine = null;

        if (!fakeLobbyManager.InRoom || !fakeLobbyManager.OpponentPresent)
        {
            yield break;
        }

        fakeLobbyManager.SetOpponentReady(true);
        RefreshRoomStatus();
    }

    private void RefreshRoomStatus()
    {
        UpdateStatusText(fakeLobbyManager.GetStatusText());
        UpdateReadyButtonVisual();
    }

    private void SetLobbyCode(string code)
    {
        if (lobbyCodeText != null)
        {
            lobbyCodeText.text = string.IsNullOrWhiteSpace(code) ? emptyLobbyCodeDisplay : code;
        }
    }

    private void UpdateStatusText(string value)
    {
        if (statusText != null)
        {
            statusText.text = string.IsNullOrWhiteSpace(value) ? defaultStatusText : value;
        }
    }

    private void SetJoinPrompt(string value, bool isError)
    {
        if (joinTitleText == null)
        {
            return;
        }

        joinTitleText.text = value;
        joinTitleText.color = isError ? joinPromptErrorColor : joinPromptNormalColor;
    }

    private void ResetJoinPrompt()
    {
        SetJoinPrompt(defaultJoinPromptText, false);
    }

    private void UpdateReadyButtonVisual()
    {
        if (readyButtonImage != null)
        {
            readyButtonImage.color = fakeLobbyManager.LocalReady ? readyButtonReadyTint : readyButtonNormalTint;
        }
    }

    private void ApplyPlayer1Portrait()
    {
        ApplyPortrait(player1PortraitImage, characterSelectManager.GetLocalPortrait(), true);
    }

    private void ApplyPlayer2Portrait()
    {
        ApplyPortrait(player2PortraitImage, characterSelectManager.GetOpponentPortrait(), true);
    }

    private void ClearPlayer2Portrait()
    {
        ApplyPortrait(player2PortraitImage, null, false);
    }

    private void ApplyPortrait(Image portraitImage, Sprite sprite, bool visible)
    {
        if (portraitImage == null)
        {
            return;
        }

        portraitImage.sprite = sprite;
        portraitImage.enabled = visible && sprite != null;

        Color color = portraitImage.color;
        color.a = portraitImage.enabled ? 1f : 0f;
        portraitImage.color = color;
    }

    private void KeepOpponentDifferentIfNeeded()
    {
        if (!fakeLobbyManager.OpponentPresent)
        {
            return;
        }

        characterSelectManager.EnsureOpponentDiffers(random);
        fakeLobbyManager.SetOpponentJoined(characterSelectManager.OpponentCharacterIndex);
        ApplyPlayer2Portrait();
        RefreshRoomStatus();
    }

    private void StopLobbySimulation()
    {
        if (fakeOpponentJoinRoutine != null)
        {
            StopCoroutine(fakeOpponentJoinRoutine);
            fakeOpponentJoinRoutine = null;
        }

        if (fakeOpponentReadyRoutine != null)
        {
            StopCoroutine(fakeOpponentReadyRoutine);
            fakeOpponentReadyRoutine = null;
        }
    }

    private void ReturnToMainMenu()
    {
        if (returnToMenuButton != null)
        {
            returnToMenuButton.ReturnToMenu();
            return;
        }

        SceneManager.LoadScene(fallbackMainMenuSceneName);
    }

    private void ShowPanels(bool showStart, bool showJoin, bool showRoom)
    {
        if (startPanel != null)
        {
            startPanel.SetActive(showStart);
        }

        if (joinPanel != null)
        {
            joinPanel.SetActive(showJoin);
        }

        if (roomPanel != null)
        {
            roomPanel.SetActive(showRoom);
        }
    }

    private void ClearLobbyCodeInput()
    {
        if (lobbyCodeInput != null)
        {
            lobbyCodeInput.text = string.Empty;
        }
    }

    private void CacheDefaultTextValues()
    {
        if (joinTitleText != null && !string.IsNullOrWhiteSpace(joinTitleText.text))
        {
            defaultJoinPromptText = joinTitleText.text;
            joinPromptNormalColor = joinTitleText.color;
        }

        if (statusText != null && !string.IsNullOrWhiteSpace(statusText.text))
        {
            defaultStatusText = statusText.text;
        }

        if (lobbyCodeText != null && !string.IsNullOrWhiteSpace(lobbyCodeText.text))
        {
            defaultLobbyCodeText = lobbyCodeText.text;
        }

        if (readyButtonImage != null)
        {
            readyButtonNormalTint = readyButtonImage.color;
        }
    }

    private void AutoAssignReferences()
    {
        startPanel = FindGameObjectByPath("Canvas/StartPanel");
        joinPanel = FindGameObjectByPath("Canvas/JoinPanel");
        roomPanel = FindGameObjectByPath("Canvas/RoomPanel");

        createLobbyButton = FindComponentByPath<Button>("Canvas/StartPanel/CreateLobbyButton");
        joinLobbyButton = FindComponentByPath<Button>("Canvas/StartPanel/JoinLobbyButton");
        backBtn = FindComponentByPath<Button>("Canvas/StartPanel/BackBtn");

        joinTitleText = FindComponentByPath<TMP_Text>("Canvas/JoinPanel/JoinTitleText");
        lobbyCodeInput = FindComponentByPath<TMP_InputField>("Canvas/JoinPanel/LobbyCodeInput");
        confirmJoinButton = FindComponentByPath<Button>("Canvas/JoinPanel/ConfirmJoinButton");
        joinBackButton = FindComponentByPath<Button>("Canvas/JoinPanel/BackButton");

        lobbyCodeText = FindComponentByPath<TMP_Text>("Canvas/RoomPanel/CodeBorderImage/LobbyCodeText");
        statusText = FindComponentByPath<TMP_Text>("Canvas/RoomPanel/StatusText");
        readyButton = FindComponentByPath<Button>("Canvas/RoomPanel/ReadyButton");
        readyButtonImage = FindComponentByPath<Image>("Canvas/RoomPanel/ReadyButton");
        leaveLobbyButton = FindComponentByPath<Button>("Canvas/RoomPanel/LeaveLobbyButton");

        playerUpButton = FindComponentByPath<Button>("Canvas/RoomPanel/Player1Slot/PlayerUp");
        playerDownButton = FindComponentByPath<Button>("Canvas/RoomPanel/Player1Slot/PlayerDown");
        player1PortraitImage = FindComponentByPath<Image>("Canvas/RoomPanel/Player1Slot/PlayerCard/PlayerPortrait");
        player2PortraitImage = FindComponentByPath<Image>("Canvas/RoomPanel/Player2Slot/PlayerCard/PlayerPortrait");

        returnToMenuButton = GetComponent<ReturnToMenuButton>();
        if (returnToMenuButton == null)
        {
            returnToMenuButton = FindComponentByPath<ReturnToMenuButton>("ReturnToMenuController");
        }
    }

    private GameObject FindGameObjectByPath(string path)
    {
        string[] pathParts = path.Split('/');
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        Transform current = null;
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == pathParts[0])
            {
                current = roots[i].transform;
                break;
            }
        }

        if (current == null)
        {
            return null;
        }

        for (int i = 1; i < pathParts.Length; i++)
        {
            current = FindDirectChildByName(current, pathParts[i]);
            if (current == null)
            {
                return null;
            }
        }

        return current.gameObject;
    }

    private static Transform FindDirectChildByName(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private T FindComponentByPath<T>(string path) where T : Component
    {
        GameObject target = FindGameObjectByPath(path);
        return target != null ? target.GetComponent<T>() : null;
    }

    private static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
}
