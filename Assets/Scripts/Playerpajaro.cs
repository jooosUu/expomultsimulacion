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
        bool controllerJump = false;
        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            controllerJump = UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame;
        }

        if (isAlive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || controllerJump))
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