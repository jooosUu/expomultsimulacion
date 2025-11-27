using UnityEngine;

public class HandPlaneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Plane plane;
    
    [Header("Control Settings")]
    [SerializeField] float pitchSensitivity = 1.5f;
    [SerializeField] float rollSensitivity = 1.5f;
    [SerializeField] float deadZone = 0.15f;
    [SerializeField] float smoothingSpeed = 8f;
    
    [Header("Throttle Settings")]
    [SerializeField] float fistThrottle = 1.0f;
    [SerializeField] float normalThrottle = 0.5f;
    [SerializeField] float openHandThrottle = 0.0f;

    [Header("Debug")]
    [SerializeField] bool showDebugInfo = false;

    private Vector3 currentInput = Vector3.zero;
    private float currentThrottle = 0f;
    private bool wasThumbsUp = false;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponentInParent<Plane>();
            if (plane == null)
            {
                Debug.LogError("❌ HandPlaneController: No Plane component found!");
                enabled = false;
            }
        }
    }

    public void UpdateHandControl(HandGestureData gestureData)
    {
        if (plane == null || plane.Dead) return;

        // Calculate Pitch and Roll from hand position
        float pitchInput = 0f;
        float rollInput = 0f;

        // Pitch (up/down) - Y axis
        if (Mathf.Abs(gestureData.position.y) > deadZone)
        {
            pitchInput = Mathf.Clamp(gestureData.position.y * pitchSensitivity, -1f, 1f);
        }

        // Roll (left/right) - X axis
        if (Mathf.Abs(gestureData.position.x) > deadZone)
        {
            rollInput = Mathf.Clamp(gestureData.position.x * rollSensitivity, -1f, 1f);
        }

        // Smooth the input for better control
        Vector3 targetInput = new Vector3(pitchInput, 0, -rollInput);
        currentInput = Vector3.Lerp(currentInput, targetInput, Time.deltaTime * smoothingSpeed);
        
        plane.SetControlInput(currentInput);

        // Handle Throttle based on gestures
        float targetThrottle = normalThrottle;
        
        if (gestureData.isOpenHand)
        {
            targetThrottle = openHandThrottle; // Brake/Slow down
        }
        else if (gestureData.isFist)
        {
            targetThrottle = fistThrottle; // Full throttle
        }
        
        currentThrottle = Mathf.Lerp(currentThrottle, targetThrottle, Time.deltaTime * 5f);
        plane.SetThrottleInput(currentThrottle);

        // Fire missiles with thumbs up gesture
        if (gestureData.isThumbsUp && !wasThumbsUp)
        {
            plane.TryFireMissile();
            if (showDebugInfo)
                Debug.Log("🚀 Firing missile!");
        }
        wasThumbsUp = gestureData.isThumbsUp;

        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"✈️ Plane Control - Pitch:{pitchInput:F2} Roll:{rollInput:F2} Throttle:{currentThrottle:F2}");
        }
    }

    void OnDisable()
    {
        // Reset controls when disabled
        if (plane != null && !plane.Dead)
        {
            plane.SetControlInput(Vector3.zero);
            plane.SetThrottleInput(0.5f);
        }
    }
}

