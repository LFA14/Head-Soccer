using UnityEngine;

public class SelectionData : MonoBehaviour
{
    public static SelectionData Instance;

    public GameObject playerPrefab;
    public GameObject comPrefab;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
