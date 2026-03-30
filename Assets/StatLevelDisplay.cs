using UnityEngine;
using UnityEngine.UI;

public class StatLevelDisplay : MonoBehaviour
{
    public Image[] levelIcons;
    public Sprite emptySprite;
    public Sprite filledSprite;

    public Color emptyColor = Color.gray;
    public Color filledColor = Color.white;

    public void SetLevel(int level)
    {
        for (int i = 0; i < levelIcons.Length; i++)
        {
            bool filled = i < level;

            if (filledSprite != null && emptySprite != null)
            {
                levelIcons[i].sprite = filled ? filledSprite : emptySprite;
            }

            levelIcons[i].color = filled ? filledColor : emptyColor;
        }
    }
}