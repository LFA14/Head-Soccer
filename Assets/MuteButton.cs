using UnityEngine;

public class MuteButton : MonoBehaviour
{
    public void ToggleMute()
    {
        if (MenuMusic.Instance != null)
            MenuMusic.Instance.ToggleMusic();
    }
}
