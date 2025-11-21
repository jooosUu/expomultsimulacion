using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private bool scored = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!scored && other.CompareTag("Player"))
        {
            scored = true;
            GameManager.instance.AddScore();
        }
    }
}