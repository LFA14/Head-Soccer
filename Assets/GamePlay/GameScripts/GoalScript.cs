using UnityEngine;

public class GoalScript : MonoBehaviour
{
    public bool isLeftGoal;
    public CountdownManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered goal trigger: " + other.name);

        if (!other.CompareTag("Ball"))
        {
            Debug.Log("It was not the ball");
            return;
        }

        Debug.Log("BALL ENTERED GOAL");

        if (isLeftGoal)
            gameManager.PlayerScored(2);
        else
            gameManager.PlayerScored(1);
    }
}