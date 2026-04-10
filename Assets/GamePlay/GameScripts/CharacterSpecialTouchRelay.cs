using UnityEngine;

public class CharacterSpecialTouchRelay : MonoBehaviour
{
    public CharacterSpecialController owner;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (owner == null || !collision.gameObject.CompareTag("Ball"))
            return;

        owner.TryTriggerSpecial(collision.rigidbody);
    }
}
