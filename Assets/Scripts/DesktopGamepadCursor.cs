using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Versión simplificada para controlar el cursor en el escritorio con el gamepad
/// Optimizado para la escena del PC
/// </summary>
public class DesktopGamepadCursor : MonoBehaviour
{
    [Header("Configuración del Cursor")]
    [SerializeField] private float cursorSpeed = 800f;
    [SerializeField] private RectTransform cursorUI; // Cursor visual opcional
    
    [Header("Joystick a usar")]
    [SerializeField] private bool useRightStick = true; // true = joystick derecho, false = izquierdo
    
    [Header("Opciones")]
    [SerializeField] private bool enableOnStart = true;
    [SerializeField] private float deadzone = 0.2f;

    private Vector2 virtualCursorPosition;
    private bool isEnabled;
    private Canvas canvas;

    void Start()
    {
        // Inicializar posición del cursor en el centro de la pantalla
        virtualCursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        // Configurar cursor del sistema
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        isEnabled = enableOnStart;

        // Buscar el canvas si hay un cursor UI
        if (cursorUI != null)
        {
            canvas = cursorUI.GetComponentInParent<Canvas>();
        }
    }

    void Update()
    {
        if (!isEnabled) return;
        if (Gamepad.current == null) return;

        // Leer input del joystick
        Vector2 stickInput = useRightStick ? 
            Gamepad.current.rightStick.ReadValue() : 
            Gamepad.current.leftStick.ReadValue();

        // Aplicar zona muerta
        if (stickInput.magnitude < deadzone)
        {
            stickInput = Vector2.zero;
        }

        // Mover cursor virtual
        virtualCursorPosition += stickInput * cursorSpeed * Time.deltaTime;

        // Limitar a los bordes de la pantalla
        virtualCursorPosition.x = Mathf.Clamp(virtualCursorPosition.x, 0, Screen.width);
        virtualCursorPosition.y = Mathf.Clamp(virtualCursorPosition.y, 0, Screen.height);

        // Actualizar cursor del sistema
        Mouse.current.WarpCursorPosition(virtualCursorPosition);

        // Actualizar cursor visual si existe
        UpdateVisualCursor();

        // Manejar clics
        HandleGamepadButtons();
    }

    void UpdateVisualCursor()
    {
        if (cursorUI == null) return;

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            cursorUI.position = virtualCursorPosition;
        }
    }

    void HandleGamepadButtons()
    {
        if (Gamepad.current == null) return;

        // Botón A = Clic izquierdo / Interactuar
        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            SimulateClick();
        }

        // Botón B = Volver / Cancelar (ya manejado por PCExitController)
    }

    void SimulateClick()
    {
        // Intentar hacer clic en elementos de UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = virtualCursorPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            // Ejecutar clic en el primer elemento encontrado
            var clickable = results[0].gameObject.GetComponent<Button>();
            if (clickable != null)
            {
                clickable.onClick.Invoke();
                Debug.Log($"Gamepad clicked: {results[0].gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Activa o desactiva el control del cursor
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    /// <summary>
    /// Cambia la velocidad del cursor
    /// </summary>
    public void SetSpeed(float speed)
    {
        cursorSpeed = speed;
    }
}
