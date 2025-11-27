using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAimController : MonoBehaviour
{
    [SerializeField] Plane plane;
    [SerializeField] Camera mainCamera;
    [SerializeField] float mouseSensitivity = 1.0f;
    [SerializeField] float virtualTargetDistance = 100f;
    [SerializeField] RectTransform mouseReticle; // UI element for mouse cursor
    [SerializeField] RectTransform flightReticle; // UI element for where plane is pointing

    private Vector3 virtualMousePosition;
    private Vector2 screenCenter;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        virtualMousePosition = mainCamera.transform.forward * virtualTargetDistance;
        
        // Lock cursor for War Thunder style
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        if (plane == null || plane.Dead) return;

        HandleMouseInput();
        UpdateReticles();
        ApplyControlInput();
    }

    void HandleMouseInput()
    {
        // Get mouse delta
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        
        // Update virtual target position in screen space logic
        // We project the current virtual target to screen, add delta, then project back
        // Or simpler: Rotate the virtual target vector based on mouse input
        
        // Rotate the virtual mouse position vector around the camera
        // Yaw (Left/Right)
        virtualMousePosition = Quaternion.AngleAxis(mouseDelta.x * 0.1f, mainCamera.transform.up) * virtualMousePosition;
        // Pitch (Up/Down)
        virtualMousePosition = Quaternion.AngleAxis(-mouseDelta.y * 0.1f, mainCamera.transform.right) * virtualMousePosition;

        // Keep it at fixed distance
        virtualMousePosition = virtualMousePosition.normalized * virtualTargetDistance;
    }

    void UpdateReticles()
    {
        if (mouseReticle != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(mainCamera.transform.position + virtualMousePosition);
            mouseReticle.position = screenPos;
        }
        
        if (flightReticle != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(plane.transform.position + plane.transform.forward * virtualTargetDistance);
            flightReticle.position = screenPos;
        }
    }

    void ApplyControlInput()
    {
        // Calculate error between plane forward and target direction
        Vector3 targetDir = (mainCamera.transform.position + virtualMousePosition - plane.transform.position).normalized;
        Vector3 currentDir = plane.transform.forward;

        // Transform to local space
        Vector3 localTarget = plane.transform.InverseTransformDirection(targetDir);

        // Calculate Pitch, Yaw, Roll inputs
        float pitch = -localTarget.y; // Pitch up/down to match target
        float yaw = localTarget.x;   // Yaw left/right
        
        // Roll logic: Roll into the turn
        // If we need to yaw right, roll right.
        float targetRoll = -yaw * 45f; // Bank angle proportional to yaw error
        float currentRoll = plane.transform.localEulerAngles.z;
        if (currentRoll > 180) currentRoll -= 360;
        
        float rollError = (targetRoll - currentRoll) / 45f;

        // Combine inputs
        // Plane.cs expects: x=Pitch, y=Yaw, z=Roll (based on PlayerController.cs logic)
        // Wait, PlayerController says: controlInput = new Vector3(input.y, controlInput.y, -input.x);
        // Let's look at Plane.cs SetControlInput: controlInput = Vector3.ClampMagnitude(input, 1);
        // And UpdateSteering uses controlInput directly.
        // Let's assume standard: x=Pitch, y=Yaw, z=Roll
        
        Vector3 input = new Vector3(
            Mathf.Clamp(pitch * 5f, -1f, 1f), // Pitch
            Mathf.Clamp(yaw * 2f, -1f, 1f),   // Yaw
            Mathf.Clamp(rollError, -1f, 1f)   // Roll
        );

        plane.SetControlInput(input);
        
        // Auto throttle
        plane.SetThrottleInput(1.0f);
    }
}
