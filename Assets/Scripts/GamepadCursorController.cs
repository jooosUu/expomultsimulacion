using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el cursor del mouse usando el joystick del control de Xbox
/// Permite navegar por el escritorio con el gamepad
/// </summary>
public class GamepadCursorController : MonoBehaviour
{
    [Header("Configuración del Cursor")]
    [Tooltip("Velocidad de movimiento del cursor con el joystick")]
    public float cursorSpeed = 1000f;
    
    [Tooltip("Zona muerta del joystick (0-1)")]
    [Range(0f, 0.5f)]
    public float deadzone = 0.15f;
    
    [Tooltip("Suavizado del movimiento del cursor")]
    [Range(0f, 1f)]
    public float smoothing = 0.5f;

    [Header("Opciones")]
    [Tooltip("Activar/desactivar el control del cursor con gamepad")]
    public bool enableGamepadCursor = true;
    
    [Tooltip("Mostrar el cursor del sistema")]
    public bool showSystemCursor = true;

    [Header("Botones (Opcional)")]
    [Tooltip("Usar botón A como clic izquierdo")]
    public bool buttonAAsLeftClick = true;
    
    [Tooltip("Usar botón B como clic derecho")]
    public bool buttonBAsRightClick = false;

    private Vector2 currentCursorPosition;
    private Vector2 smoothVelocity;

    void Start()
    {
        // Obtener la posición inicial del cursor
        currentCursorPosition = Mouse.current.position.ReadValue();
        
        // Configurar visibilidad del cursor
        Cursor.visible = showSystemCursor;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (!enableGamepadCursor) return;

        // Verificar si hay un gamepad conectado
        if (Gamepad.current == null) return;

        // Leer el input del joystick derecho (o izquierdo, según prefieras)
        Vector2 joystickInput = Gamepad.current.rightStick.ReadValue();
        
        // Aplicar zona muerta
        if (joystickInput.magnitude < deadzone)
        {
            joystickInput = Vector2.zero;
        }

        // Calcular el movimiento del cursor
        Vector2 cursorDelta = joystickInput * cursorSpeed * Time.deltaTime;
        
        // Aplicar suavizado
        if (smoothing > 0)
        {
            cursorDelta = Vector2.SmoothDamp(Vector2.zero, cursorDelta, ref smoothVelocity, smoothing * Time.deltaTime);
        }

        // Actualizar posición del cursor
        currentCursorPosition += cursorDelta;
        
        // Limitar el cursor a los límites de la pantalla
        currentCursorPosition.x = Mathf.Clamp(currentCursorPosition.x, 0, Screen.width);
        currentCursorPosition.y = Mathf.Clamp(currentCursorPosition.y, 0, Screen.height);

        // Mover el cursor del sistema
        Mouse.current.WarpCursorPosition(currentCursorPosition);

        // Manejar clics con botones del gamepad (opcional)
        HandleGamepadClicks();
    }

    void HandleGamepadClicks()
    {
        if (Gamepad.current == null) return;

        // Botón A como clic izquierdo
        if (buttonAAsLeftClick && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            SimulateMouseClick(0); // 0 = clic izquierdo
        }

        // Botón B como clic derecho
        if (buttonBAsRightClick && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            SimulateMouseClick(1); // 1 = clic derecho
        }
    }

    void SimulateMouseClick(int button)
    {
        // Nota: Unity no puede simular clics del mouse del sistema directamente
        // Esta función está aquí para detectar el input del gamepad
        // Los clics se pueden manejar mediante Raycasts en la UI o en objetos 3D
        
        Debug.Log($"Gamepad button pressed - simulating mouse button {button}");
        
        // Aquí podrías agregar lógica personalizada para interactuar con UI
        // Por ejemplo, hacer raycast y detectar botones de UI
        DetectUIClick();
    }

    void DetectUIClick()
    {
        // Hacer raycast desde la posición del cursor
        Ray ray = Camera.main.ScreenPointToRay(currentCursorPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Detectar si se hizo clic en un objeto interactivo
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"Clicked on: {hit.collider.gameObject.name}");
                // Aquí puedes llamar a métodos de interacción
            }
        }
    }

    /// <summary>
    /// Activa o desactiva el control del cursor con gamepad
    /// </summary>
    public void SetGamepadCursorEnabled(bool enabled)
    {
        enableGamepadCursor = enabled;
    }

    /// <summary>
    /// Cambia la velocidad del cursor
    /// </summary>
    public void SetCursorSpeed(float speed)
    {
        cursorSpeed = Mathf.Max(0, speed);
    }

    /// <summary>
    /// Obtiene la posición actual del cursor
    /// </summary>
    public Vector2 GetCursorPosition()
    {
        return currentCursorPosition;
    }

    // Visualización en el editor (opcional)
    void OnGUI()
    {
        if (!enableGamepadCursor || Gamepad.current == null) return;

        // Mostrar información de debug
        if (Application.isEditor)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 300, 20), $"Gamepad Cursor: {currentCursorPosition}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Joystick: {Gamepad.current.rightStick.ReadValue()}");
        }
    }
}

// Interfaz para objetos interactivos (opcional)
public interface IInteractable
{
    void OnInteract();
}
