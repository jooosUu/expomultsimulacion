using UnityEngine;

public class Playerpajaro : MonoBehaviour
{
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private bool isAlive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isAlive && Input.GetMouseButtonDown(0))
        {
            Jump();
        }
    }

    void Jump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(0, jumpForce);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Ground"))
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false;
        GameManager.instance.GameOver();
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}