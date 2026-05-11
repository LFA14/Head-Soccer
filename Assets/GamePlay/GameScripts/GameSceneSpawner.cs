using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public Transform playerSpawnPoint;
    public Transform aiSpawnPoint;

    [Header("Tournament Fallback Prefabs (same order as tournament portraits)")]
    public GameObject[] tournamentCharacterPrefabs;

    [Header("Photon")]
    public string photonResourcesCharacterFolder = "Characters";

    private bool spawnedOnlinePlayer;

    void Start()
    {
        if (GameModeManager.IsOnlineMatch && PhotonNetwork.InRoom)
        {
            SpawnOnlineMatch();
            return;
        }

        bool shouldSpawnTournamentMatch =
            MatchContext.Instance != null &&
            MatchContext.Instance.currentMode == MatchContext.MatchMode.Tournament &&
            TournamentStateData.Instance != null &&
            TournamentSelectionData.Instance != null &&
            TournamentStateData.Instance.nextOpponentIndex >= 0;

        if (shouldSpawnTournamentMatch)
        {
            SpawnTournamentMatch();
        }
        else
        {
            SpawnNormalMatch();
        }
    }

    void SpawnOnlineMatch()
    {
        if (spawnedOnlinePlayer)
            return;

        if (!GameModeManager.IsOnlineMatch)
            return;

        if (tournamentCharacterPrefabs == null || tournamentCharacterPrefabs.Length == 0)
        {
            Debug.LogError("Online character prefabs are not assigned.");
            return;
        }

        int selectedIndex = PhotonLobbyKeys.GetSelectedCharacterIndex(
            PhotonNetwork.LocalPlayer,
            0,
            tournamentCharacterPrefabs.Length);

        if (selectedIndex < 0 || selectedIndex >= tournamentCharacterPrefabs.Length)
        {
            Debug.LogWarning("Online selected character index was out of range. Falling back to index 0.");
            selectedIndex = 0;
        }

        GameObject selectedPrefab = tournamentCharacterPrefabs[selectedIndex];
        if (selectedPrefab == null)
        {
            Debug.LogError("Online selected prefab is missing for index " + selectedIndex);
            return;
        }

        Player[] orderedPlayers = PhotonNetwork.PlayerList.OrderBy(player => player.ActorNumber).ToArray();
        int localPlayerOrder = Array.FindIndex(
            orderedPlayers,
            player => player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        bool usePrimarySpawn = localPlayerOrder <= 0;
        Transform assignedSpawnPoint = usePrimarySpawn ? playerSpawnPoint : aiSpawnPoint;
        Transform opposingSpawnPoint = usePrimarySpawn ? aiSpawnPoint : playerSpawnPoint;

        if (assignedSpawnPoint == null)
        {
            Debug.LogError("Assigned online spawn point is missing.");
            return;
        }

        bool playerIsOnRightSide = opposingSpawnPoint != null &&
                                   assignedSpawnPoint.position.x > opposingSpawnPoint.position.x;

        if (MatchContext.Instance != null)
        {
            MatchContext.Instance.SetMode(MatchContext.MatchMode.Online);
            MatchContext.Instance.SetPlayerSide(playerIsOnRightSide);
        }

        string resourcePath = BuildPhotonResourcePath(selectedPrefab);
        Vector3 spawnPosition = assignedSpawnPoint.position + new Vector3(0f, 1f, 0f);
        PhotonNetwork.Instantiate(
            resourcePath,
            spawnPosition,
            Quaternion.identity,
            0,
            new object[] { usePrimarySpawn ? 0 : 1 });

        spawnedOnlinePlayer = true;
    }

    string BuildPhotonResourcePath(GameObject prefab)
    {
        if (string.IsNullOrWhiteSpace(photonResourcesCharacterFolder))
            return prefab.name;

        return photonResourcesCharacterFolder.TrimEnd('/') + "/" + prefab.name;
    }

    void SpawnNormalMatch()
    {
        if (SelectionData.Instance == null)
        {
            Debug.LogError("SelectionData.Instance is null in GameScene.");
            return;
        }

        if (SelectionData.Instance.playerPrefab == null)
        {
            Debug.LogError("Selected player prefab is null.");
            return;
        }

        if (SelectionData.Instance.comPrefab == null)
        {
            Debug.LogError("Selected com prefab is null.");
            return;
        }

        GameObject player = Instantiate(
            SelectionData.Instance.playerPrefab,
            playerSpawnPoint.position + new Vector3(0f, 1f, 0f),
            Quaternion.identity
        );

        GameObject ai = Instantiate(
            SelectionData.Instance.comPrefab,
            aiSpawnPoint.position + new Vector3(0f, 1f, 0f),
            Quaternion.identity
        );

        if (MatchContext.Instance != null)
            MatchContext.Instance.SetPlayerSide(playerSpawnPoint.position.x > aiSpawnPoint.position.x);

        SetupSpawnedCharacters(player, ai);
    }

    void SpawnTournamentMatch()
    {
        if (tournamentCharacterPrefabs == null || tournamentCharacterPrefabs.Length == 0)
        {
            Debug.LogError("Tournament character prefabs are not assigned.");
            return;
        }

        int playerIndex = TournamentSelectionData.Instance.playerIndex;
        int opponentIndex = TournamentStateData.Instance.nextOpponentIndex;

        if (playerIndex < 0 || playerIndex >= tournamentCharacterPrefabs.Length)
        {
            Debug.LogError("Tournament player index is invalid: " + playerIndex);
            return;
        }

        if (opponentIndex < 0 || opponentIndex >= tournamentCharacterPrefabs.Length)
        {
            Debug.LogError("Tournament opponent index is invalid: " + opponentIndex);
            return;
        }

        GameObject playerPrefab = tournamentCharacterPrefabs[playerIndex];
        GameObject opponentPrefab = tournamentCharacterPrefabs[opponentIndex];

        if (playerPrefab == null || opponentPrefab == null)
        {
            Debug.LogError("Tournament prefab missing.");
            return;
        }

        GameObject player = Instantiate(
            playerPrefab,
            playerSpawnPoint.position + new Vector3(0f, 1f, 0f),
            Quaternion.identity
        );

        GameObject ai = Instantiate(
            opponentPrefab,
            aiSpawnPoint.position + new Vector3(0f, 1f, 0f),
            Quaternion.identity
        );

        if (MatchContext.Instance != null)
            MatchContext.Instance.SetPlayerSide(playerSpawnPoint.position.x > aiSpawnPoint.position.x);

        SetupSpawnedCharacters(player, ai);

        Debug.Log("Tournament match spawned. Player index: " + playerIndex + " Opponent index: " + opponentIndex);
    }

    void SetupSpawnedCharacters(GameObject player, GameObject ai)
    {
        if (player == null || ai == null)
            return;

        Vector3 playerScale = player.transform.localScale;
        playerScale.x = Mathf.Abs(playerScale.x);
        player.transform.localScale = playerScale;

        Vector3 aiScale = ai.transform.localScale;
        aiScale.x = -Mathf.Abs(aiScale.x);
        ai.transform.localScale = aiScale;

        PlayerMovement playerMove = player.GetComponentInChildren<PlayerMovement>();
        if (playerMove != null)
        {
            playerMove.isPlayer = true;
            playerMove.enabled = true;
        }

        SimpleAI playerAI = player.GetComponentInChildren<SimpleAI>();
        if (playerAI != null)
        {
            playerAI.isAI = false;
            playerAI.enabled = true;
        }

        KickController playerKick = player.GetComponentInChildren<KickController>();
        if (playerKick != null)
        {
            playerKick.isPlayer = true;
        }

        CharacterSpecialController playerSpecial = player.GetComponent<CharacterSpecialController>();
        if (playerSpecial == null)
            playerSpecial = player.AddComponent<CharacterSpecialController>();

        playerSpecial.Configure(true);
        PlayerCustomizationApplier.ApplyToPlayer(player);

        PlayerMovement aiMove = ai.GetComponentInChildren<PlayerMovement>();
        if (aiMove != null)
        {
            aiMove.isPlayer = false;
            aiMove.enabled = true;
        }

        SimpleAI aiBrain = ai.GetComponentInChildren<SimpleAI>();
        if (aiBrain != null)
        {
            aiBrain.isAI = true;
            aiBrain.enabled = true;
            aiBrain.homeX = aiSpawnPoint.position.x;
            aiBrain.attackRightGoal = false;
        }

        KickController aiKick = ai.GetComponentInChildren<KickController>();
        if (aiKick != null)
        {
            aiKick.isPlayer = false;
        }

        CharacterSpecialController aiSpecial = ai.GetComponent<CharacterSpecialController>();
        if (aiSpecial == null)
            aiSpecial = ai.AddComponent<CharacterSpecialController>();

        aiSpecial.Configure(false);
    }
}
