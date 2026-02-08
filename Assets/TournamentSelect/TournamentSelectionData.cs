using UnityEngine;

public class TournamentSelectionData : MonoBehaviour
{
    public static TournamentSelectionData Instance;

    public GameObject playerPrefab;   // the chosen character prefab
    public int playerIndex;           // optional, useful later

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
