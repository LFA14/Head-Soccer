using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public Transform playerSpawnPoint;
    public Transform aiSpawnPoint;

    [Header("Tournament Fallback Prefabs (same order as tournament portraits)")]
    public GameObject[] tournamentCharacterPrefabs;

    void Start()
    {
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
    }
}
