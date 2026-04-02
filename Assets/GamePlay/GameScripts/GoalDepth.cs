using UnityEngine;

public class GoalDepth : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Ball"))
        {
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -1; // behind the net
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Ball"))
        {
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1; // back in front
        }
    }
}