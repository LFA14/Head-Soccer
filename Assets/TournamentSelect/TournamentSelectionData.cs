using UnityEngine;

public class TournamentSelectionData : MonoBehaviour
{
    public static TournamentSelectionData Instance;

    public GameObject playerPrefab;   // chosen character prefab
    public int playerIndex;           // chosen character index

    // NEW: store the 4 slots for the bracket (indexes into your arrays)
    public int[] bracketIndexes = new int[4];  // [0]=TL player, [1]=TR, [2]=BL, [3]=BR

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}