using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    
    [Header("Referencias")]
    public Rigidbody2D rb;
    public Animator animator; // Opcional: para animaciones
    
    private Vector2 movement;
    private Vector2 currentVelocity;

    void Start()
    {
        // Si no se asignó el Rigidbody2D, lo buscamos automáticamente
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        // Configurar el Rigidbody2D para movimiento suave
        if (rb != null)
        {
            rb.gravityScale = 0f; // Importante para 2D sin gravedad
            rb.freezeRotation = true; // Congelar rotación
        }
    }

    void Update()
    {
        // Detectar input del teclado
        ProcessInput();
        
        // Actualizar animaciones (si hay animator)
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Mover el personaje usando física (mejor para colliders)
        MoveCharacter();
    }

    void ProcessInput()
    {
        // Obtener input horizontal (A/D o flechas izquierda/derecha)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Obtener input vertical (W/S o flechas arriba/abajo)
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // Normalizar el vector para movimiento diagonal consistente
        movement = new Vector2(horizontalInput, verticalInput).normalized;
    }

    void MoveCharacter()
    {
        if (rb == null) return;

        // Movimiento suave con aceleración y desaceleración
        Vector2 targetVelocity = movement * moveSpeed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, 
                                        movement.magnitude > 0 ? 1f / acceleration : 1f / deceleration);
    }

    void UpdateAnimations()
    {
        // Si tienes un Animator, actualiza los parámetros aquí
        if (animator != null)
        {
            bool isMoving = movement.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
            
            // Actualizar dirección para animaciones
            if (isMoving)
            {
                animator.SetFloat("MoveX", movement.x);
                animator.SetFloat("MoveY", movement.y);
            }
        }
    }

    // Método para obtener si el personaje se está moviendo (útil para otros scripts)
    public bool IsMoving()
    {
        return movement.magnitude > 0.1f;
    }

    // Método para obtener la dirección del movimiento
    public Vector2 GetMovementDirection()
    {
        return movement;
    }
}