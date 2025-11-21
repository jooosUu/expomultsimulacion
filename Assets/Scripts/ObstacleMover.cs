using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPlaying)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }
}