using UnityEngine;

public class TournamentStateData : MonoBehaviour
{
    public static TournamentStateData Instance;

    // indices into your portraits/prefabs arrays
    public int tl; // player
    public int bl; // player opponent
    public int tr;
    public int br;

    public int finalLeft;
    public int finalRight;

    public int champion = -1;

    // progress flags
    public bool bracketGenerated = false;   // shuffle finished once
    public bool otherMatchResolved = false; // TR vs BR winner chosen
    public bool playerMatchResolved = false; // TL vs BL finished in gameplay
    public bool finalResolved = false;

    // who you should fight next
    public int nextOpponentIndex = -1; // set before going to GameScene

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}