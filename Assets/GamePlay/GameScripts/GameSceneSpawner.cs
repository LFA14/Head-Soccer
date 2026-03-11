using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public Transform playerSpawnPoint;

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

        Vector3 spawnPos = playerSpawnPoint.position + new Vector3(0, 1f, 0);

        Instantiate(
            SelectionData.Instance.playerPrefab,
            spawnPos,
            Quaternion.identity
        );

        Debug.Log("Spawned player: " + SelectionData.Instance.playerPrefab.name);
    }
}