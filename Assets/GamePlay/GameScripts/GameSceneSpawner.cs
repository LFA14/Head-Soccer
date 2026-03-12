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
            Debug.LogError("Selected AI prefab is null.");
            return;
        }

        Vector3 playerPos = playerSpawnPoint.position + new Vector3(0, 1f, 0);
        Vector3 aiPos = aiSpawnPoint.position + new Vector3(0, 1f, 0);

        GameObject player = Instantiate(
            SelectionData.Instance.playerPrefab,
            playerPos,
            Quaternion.identity
        );

        GameObject ai = Instantiate(
            SelectionData.Instance.comPrefab,
            aiPos,
            Quaternion.identity
        );

        Vector3 pScale = player.transform.localScale;
        pScale.x = Mathf.Abs(pScale.x);
        player.transform.localScale = pScale;

        Vector3 aScale = ai.transform.localScale;
        aScale.x = -Mathf.Abs(aScale.x);
        ai.transform.localScale = aScale;

        PlayerMovement pMove = player.GetComponentInChildren<PlayerMovement>();
        if (pMove != null)
        {
            pMove.isPlayer = true;
            pMove.enabled = true;
        }

        PlayerMovement aMove = ai.GetComponentInChildren<PlayerMovement>();
        if (aMove != null)
        {
            aMove.isPlayer = false;
            aMove.enabled = false;
        }

        KickController pKick = player.GetComponentInChildren<KickController>();
        if (pKick != null)
        {
            pKick.isPlayer = true;
            pKick.enabled = true;
        }

        KickController aKick = ai.GetComponentInChildren<KickController>();
        if (aKick != null)
        {
            aKick.isPlayer = false;
            aKick.enabled = true;
        }
    }
}