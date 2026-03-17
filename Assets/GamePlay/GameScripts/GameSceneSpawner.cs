using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public Transform playerSpawnPoint;
    public Transform aiSpawnPoint;

    void Start()
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
        }

        SimpleAI playerAI = player.GetComponentInChildren<SimpleAI>();
        if (playerAI != null)
        {
            playerAI.isAI = false;
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
        }

        SimpleAI aiBrain = ai.GetComponentInChildren<SimpleAI>();
        if (aiBrain != null)
        {
            aiBrain.isAI = true;
            aiBrain.homeX = aiSpawnPoint.position.x;
        }

        KickController aiKick = ai.GetComponentInChildren<KickController>();
        if (aiKick != null)
        {
            aiKick.isPlayer = false;
        }
    }
}