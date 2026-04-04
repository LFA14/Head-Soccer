using UnityEngine;

public class TournamentMatchSpawner : MonoBehaviour
{
    [Header("Character Prefabs - same order as selection/bracket")]
    public GameObject[] characterPrefabs;

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform opponentSpawnPoint;

    [Header("Spawn Offsets")]
    public Vector3 playerOffset = Vector3.zero;
    public Vector3 opponentOffset = Vector3.zero;

    private void Start()
    {
        SpawnMatch();
    }

    void SpawnMatch()
    {
        if (TournamentSelectionData.Instance == null)
        {
            Debug.LogError("TournamentSelectionData.Instance is missing.");
            return;
        }

        if (TournamentStateData.Instance == null)
        {
            Debug.LogError("TournamentStateData.Instance is missing.");
            return;
        }

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("characterPrefabs is empty.");
            return;
        }

        int playerIndex = TournamentSelectionData.Instance.playerIndex;
        int opponentIndex = TournamentStateData.Instance.nextOpponentIndex;

        if (playerIndex < 0 || playerIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Invalid playerIndex: " + playerIndex);
            return;
        }

        if (opponentIndex < 0 || opponentIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Invalid opponentIndex: " + opponentIndex);
            return;
        }

        GameObject playerObj = Instantiate(
            characterPrefabs[playerIndex],
            playerSpawnPoint.position + playerOffset,
            Quaternion.identity
        );

        GameObject opponentObj = Instantiate(
            characterPrefabs[opponentIndex],
            opponentSpawnPoint.position + opponentOffset,
            Quaternion.identity
        );

        SetupPlayer(playerObj);
        SetupOpponent(opponentObj);

        Debug.Log("Spawned player index: " + playerIndex);
        Debug.Log("Spawned opponent index: " + opponentIndex);
    }

    void SetupPlayer(GameObject obj)
    {
        if (obj == null) return;

        SimpleAI ai = obj.GetComponentInChildren<SimpleAI>(true);
        if (ai != null)
            ai.isAI = false;

        PlayerMovement movement = obj.GetComponentInChildren<PlayerMovement>(true);
        if (movement != null)
            movement.enabled = true;

        KickController kick = obj.GetComponentInChildren<KickController>(true);
        if (kick != null)
            kick.isPlayer = true;
    }

    void SetupOpponent(GameObject obj)
    {
        if (obj == null) return;

        SimpleAI ai = obj.GetComponentInChildren<SimpleAI>(true);
        if (ai != null)
            ai.isAI = true;

        PlayerMovement movement = obj.GetComponentInChildren<PlayerMovement>(true);
        if (movement != null)
            movement.enabled = false;

        KickController kick = obj.GetComponentInChildren<KickController>(true);
        if (kick != null)
            kick.isPlayer = false;
    }
}