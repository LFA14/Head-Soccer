using UnityEngine;
using UnityEngine.UI;
public class AlphaHitTestEnabler : MonoBehaviour
{
public Image targetImage;
    
    void Start()
    {
        if (targetImage != null)
        {
            targetImage.alphaHitTestMinimumThreshold = 0.1f; // Set threshold as needed
        }
    }
}
    
