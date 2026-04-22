using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OnlineLobbyUIManager : MonoBehaviourPunCallbacks
{
    private const string ConnectingStatus = "Connecting...";
    private const string ConnectedStatus = "Connected!";
    private const string WaitingForPlayerStatus = "Waiting for player...";
    private const string OpponentJoinedStatus = "Opponent joined!";
    private const string PlayerLeftStatus = "Player left. Waiting for player...";
    private const string ReadyStatus = "Ready!";
    private const string NotReadyStatus = "Not Ready";
    private const string BothPlayersReadyStatus = "Both players ready";
    private const string WaitingForOpponentReadyStatus = "Ready! Waiting for opponent...";
    private const string WaitingForPlayerWhileReadyStatus = "Ready! Waiting for player...";
    private const string OpponentReadyStatus = "Opponent ready. You are not ready.";
    private const byte MaxPlayersPerRoom = 2;
    private const int MaxCreateRetries = 5;

    private enum PendingLobbyAction
    {
        None,
        CreateRoom,
        JoinRoom
    }

    [Header("Character Order")]
    [SerializeField] private Sprite[] portraitSprites;
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("UI Colors")]
    [SerializeField] private Color readyButtonNormalTint = Color.white;
    [SerializeField] private Color readyButtonReadyTint = new Color(0.75f, 1f, 0.75f, 1f);
    [SerializeField] private Color joinPromptNormalColor = Color.white;
    [SerializeField] private Color joinPromptErrorColor = new Color(1f, 0.4f, 0.4f, 1f);

    [Header("Fallbacks")]
    [SerializeField] private string fallbackMainMenuSceneName = "MenuScene";
    [SerializeField] private string emptyLobbyCodeDisplay = "------";
    [SerializeField] private string gameSceneName = "GameScene";

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
    private LobbyCharacterSelectManager characterSelectManager;

    private string defaultJoinPromptText = "Enter Lobby Code";
    private string defaultStatusText = NotReadyStatus;
    private string defaultLobbyCodeText = "------";
    private bool listenersRegistered;
    private bool loadingGameScene;
    private PendingLobbyAction pendingAction;
    private string pendingRoomCode = string.Empty;
    private int createRetryCount;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        AutoAssignReferences();
        CacheDefaultTextValues();

        characterSelectManager = new LobbyCharacterSelectManager(portraitSprites, characterPrefabs);

        RegisterListeners();
        ApplyLocalPortrait();
        ClearRemotePortrait();
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            loadingGameScene = false;
            RefreshLobbyUi();
            return;
        }

        ResetToStartPanel();
        EnsureConnectedToPhoton();
    }

    private void OnDestroy()
    {
        UnregisterListeners();
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
        pendingAction = PendingLobbyAction.CreateRoom;
        pendingRoomCode = GenerateRoomCode();
        createRetryCount = 0;
        SetJoinPrompt(defaultJoinPromptText, false);
        EnsureConnectedToPhoton();

        if (PhotonNetwork.IsConnectedAndReady)
        {
            CreateRoomWithCode(pendingRoomCode);
        }
    }

    private void OnJoinLobbyPressed()
    {
        pendingAction = PendingLobbyAction.None;
        pendingRoomCode = string.Empty;
        createRetryCount = 0;
        ResetJoinPrompt();
        ShowPanels(showStart: false, showJoin: true, showRoom: false);
    }

    private void OnConfirmJoinPressed()
    {
        string normalizedCode = PhotonLobbyKeys.NormalizeRoomCode(lobbyCodeInput != null ? lobbyCodeInput.text : string.Empty);
        if (string.IsNullOrEmpty(normalizedCode))
        {
            SetJoinPrompt("Please enter a lobby code.", true);
            return;
        }

        pendingAction = PendingLobbyAction.JoinRoom;
        pendingRoomCode = normalizedCode;
        ResetJoinPrompt();
        EnsureConnectedToPhoton();

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinRoomWithCode(pendingRoomCode);
        }
    }

    private void OnReadyPressed()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        bool nextReadyState = !PhotonLobbyKeys.GetReadyState(PhotonNetwork.LocalPlayer);
        SetLocalPlayerProperties(characterSelectManager.LocalCharacterIndex, nextReadyState);
        RefreshLobbyUi();
    }

    private void OnLeaveLobbyPressed()
    {
        pendingAction = PendingLobbyAction.None;
        pendingRoomCode = string.Empty;
        loadingGameScene = false;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            ResetToStartPanel();
        }
    }

    private void OnPlayerUpPressed()
    {
        characterSelectManager.MovePrevious();
        ApplyLocalPortrait();
        PushLocalCharacterSelection();
    }

    private void OnPlayerDownPressed()
    {
        characterSelectManager.MoveNext();
        ApplyLocalPortrait();
        PushLocalCharacterSelection();
    }

    private void OnLobbyCodeSubmitted(string _)
    {
        if (joinPanel != null && joinPanel.activeSelf)
        {
            OnConfirmJoinPressed();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (pendingAction == PendingLobbyAction.CreateRoom)
        {
            CreateRoomWithCode(pendingRoomCode);
            return;
        }

        if (pendingAction == PendingLobbyAction.JoinRoom)
        {
            JoinRoomWithCode(pendingRoomCode);
        }
    }

    public override void OnCreatedRoom()
    {
        UpdateStatusText(WaitingForPlayerStatus);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (pendingAction == PendingLobbyAction.CreateRoom && createRetryCount < MaxCreateRetries)
        {
            createRetryCount++;
            pendingRoomCode = GenerateRoomCode();
            CreateRoomWithCode(pendingRoomCode);
            return;
        }

        pendingAction = PendingLobbyAction.None;
        pendingRoomCode = string.Empty;
        ShowPanels(showStart: true, showJoin: false, showRoom: false);
        SetJoinPrompt("Could not create room. Try again.", true);
        Debug.LogWarning($"Create room failed ({returnCode}): {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        pendingAction = PendingLobbyAction.None;
        ShowPanels(showStart: false, showJoin: true, showRoom: false);
        SetJoinPrompt("Room not found. Check the code.", true);
        Debug.LogWarning($"Join room failed ({returnCode}): {message}");
    }

    public override void OnJoinedRoom()
    {
        pendingAction = PendingLobbyAction.None;
        createRetryCount = 0;
        loadingGameScene = false;

        ShowPanels(showStart: false, showJoin: false, showRoom: true);
        SetLobbyCode(PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : pendingRoomCode);

        SetLocalPlayerProperties(characterSelectManager.LocalCharacterIndex, false);

        if (MatchContext.Instance != null)
        {
            MatchContext.Instance.SetMode(MatchContext.MatchMode.Online);
        }

        RefreshLobbyUi();
        TryStartGameIfBothPlayersReady();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        loadingGameScene = false;
        RefreshLobbyUi();
        TryStartGameIfBothPlayersReady();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        loadingGameScene = false;
        RefreshLobbyUi();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonLobbyKeys.ContainsLobbyProperties(changedProps))
        {
            return;
        }

        RefreshLobbyUi();
        TryStartGameIfBothPlayersReady();
    }

    public override void OnLeftRoom()
    {
        loadingGameScene = false;
        ResetToStartPanel();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loadingGameScene = false;

        if (SceneManager.GetActiveScene().name == "OnlineLobbyScene")
        {
            ResetToStartPanel();
            SetJoinPrompt("Disconnected from Photon.", true);
        }

        Debug.LogWarning("Photon disconnected: " + cause);
    }

    private void EnsureConnectedToPhoton()
    {
        if (PhotonNetwork.IsConnectedAndReady || PhotonNetwork.IsConnected)
        {
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    private void CreateRoomWithCode(string code)
    {
        if (!PhotonNetwork.IsConnectedAndReady || string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        pendingRoomCode = code;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MaxPlayersPerRoom,
            CleanupCacheOnLeave = true,
            PublishUserId = true
        };

        PhotonNetwork.CreateRoom(code, roomOptions, TypedLobby.Default);
    }

    private void JoinRoomWithCode(string code)
    {
        if (!PhotonNetwork.IsConnectedAndReady || string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        pendingRoomCode = code;
        PhotonNetwork.JoinRoom(code);
    }

    private void PushLocalCharacterSelection()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        bool currentReadyState = PhotonLobbyKeys.GetReadyState(PhotonNetwork.LocalPlayer);
        SetLocalPlayerProperties(characterSelectManager.LocalCharacterIndex, currentReadyState);
    }

    private void SetLocalPlayerProperties(int characterIndex, bool readyState)
    {
        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        Hashtable properties = PhotonLobbyKeys.CreateLobbyProperties(characterIndex, readyState);
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private void TryStartGameIfBothPlayersReady()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || loadingGameScene)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount != MaxPlayersPerRoom)
        {
            return;
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (!PhotonLobbyKeys.GetReadyState(players[i]))
            {
                return;
            }
        }

        loadingGameScene = true;
        UpdateStatusText(BothPlayersReadyStatus);

        if (MatchContext.Instance != null)
        {
            MatchContext.Instance.SetMode(MatchContext.MatchMode.Online);
        }

        PhotonNetwork.LoadLevel(gameSceneName);
    }

    private void RefreshLobbyUi()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            ApplyLocalPortrait();
            ClearRemotePortrait();
            UpdateReadyButtonVisual(false);
            return;
        }

        ShowPanels(showStart: false, showJoin: false, showRoom: true);
        SetLobbyCode(PhotonNetwork.CurrentRoom.Name);

        int localCharacterIndex = PhotonLobbyKeys.GetSelectedCharacterIndex(
            PhotonNetwork.LocalPlayer,
            characterSelectManager.LocalCharacterIndex,
            characterSelectManager.CharacterCount);

        characterSelectManager.SetLocalIndex(localCharacterIndex);
        ApplyLocalPortrait();

        Player remotePlayer = GetRemotePlayer();
        if (remotePlayer != null)
        {
            int remoteCharacterIndex = PhotonLobbyKeys.GetSelectedCharacterIndex(remotePlayer, 0, characterSelectManager.CharacterCount);
            characterSelectManager.SetOpponentIndex(remoteCharacterIndex);
            ApplyRemotePortrait();
        }
        else
        {
            characterSelectManager.ClearOpponent();
            ClearRemotePortrait();
        }

        bool localReady = PhotonLobbyKeys.GetReadyState(PhotonNetwork.LocalPlayer);
        UpdateReadyButtonVisual(localReady);
        UpdateStatusText(BuildRoomStatusText(localReady, remotePlayer));
    }

    private string BuildRoomStatusText(bool localReady, Player remotePlayer)
    {
        if (loadingGameScene)
        {
            return BothPlayersReadyStatus;
        }

        if (!PhotonNetwork.IsConnected)
        {
            return ConnectingStatus;
        }

        if (!PhotonNetwork.InRoom)
        {
            return ConnectedStatus;
        }

        if (remotePlayer == null)
        {
            return localReady ? WaitingForPlayerWhileReadyStatus : WaitingForPlayerStatus;
        }

        bool remoteReady = PhotonLobbyKeys.GetReadyState(remotePlayer);
        if (localReady && remoteReady)
        {
            return BothPlayersReadyStatus;
        }

        if (localReady)
        {
            return WaitingForOpponentReadyStatus;
        }

        if (remoteReady)
        {
            return OpponentReadyStatus;
        }

        return PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom ? OpponentJoinedStatus : PlayerLeftStatus;
    }

    private Player GetRemotePlayer()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (PhotonNetwork.LocalPlayer == null || players[i].ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return players[i];
            }
        }

        return null;
    }

    private void ResetToStartPanel()
    {
        ShowPanels(showStart: true, showJoin: false, showRoom: false);
        ResetJoinPrompt();
        ClearLobbyCodeInput();
        SetLobbyCode(defaultLobbyCodeText);
        UpdateStatusText(defaultStatusText);
        characterSelectManager.ClearOpponent();
        ApplyLocalPortrait();
        ClearRemotePortrait();
        UpdateReadyButtonVisual(false);
    }

    private void ApplyLocalPortrait()
    {
        ApplyPortrait(player1PortraitImage, characterSelectManager.GetLocalPortrait(), true);
    }

    private void ApplyRemotePortrait()
    {
        ApplyPortrait(player2PortraitImage, characterSelectManager.GetOpponentPortrait(), true);
    }

    private void ClearRemotePortrait()
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

    private void UpdateReadyButtonVisual(bool isReady)
    {
        if (readyButtonImage != null)
        {
            readyButtonImage.color = isReady ? readyButtonReadyTint : readyButtonNormalTint;
        }
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

    private string GenerateRoomCode()
    {
        return Random.Range(100000, 999999).ToString();
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

    private void ReturnToMainMenu()
    {
        if (returnToMenuButton != null)
        {
            returnToMenuButton.ReturnToMenu();
            return;
        }

        SceneManager.LoadScene(fallbackMainMenuSceneName);
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
