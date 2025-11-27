using UnityEngine;

public class HandFlappyBirdController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Playerpajaro player;
    
    [Header("Control Settings")]
    [Tooltip("Altura mínima de la mano para hacer saltar al pájaro (0 a 1, donde 0.2 = 20% desde el centro hacia arriba)")]
    [SerializeField] float jumpThreshold = 0.2f;
    
    [Tooltip("Tiempo mínimo entre saltos (segundos)")]
    [SerializeField] float jumpCooldown = 0.2f;
    
    [Header("Gesture Triggers")]
    [SerializeField] bool jumpWithHandUp = true;
    [SerializeField] bool jumpWithOpenHand = true;
    [SerializeField] bool jumpWithFist = false;

    [Header("Debug")]
    [SerializeField] bool showDebugInfo = false;

    private float lastJumpTime = 0f;
    private bool wasAboveThreshold = false;

    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Playerpajaro>();
            if (player == null)
            {
                Debug.LogError("❌ HandFlappyBirdController: No Playerpajaro found in scene!");
                enabled = false;
            }
        }
    }

    public void UpdateHandControl(HandGestureData gestureData)
    {
        if (player == null || !player.IsAlive()) return;

        bool shouldJump = false;

        // Method 1: Jump when hand goes above threshold
        if (jumpWithHandUp)
        {
            bool isAboveThreshold = gestureData.position.y > jumpThreshold;
            
            // Trigger jump on rising edge (hand crosses threshold upward)
            if (isAboveThreshold && !wasAboveThreshold)
            {
                shouldJump = true;
            }
            
            wasAboveThreshold = isAboveThreshold;
        }

        // Method 2: Jump with open hand gesture
        if (jumpWithOpenHand && gestureData.isOpenHand)
        {
            shouldJump = true;
        }

        // Method 3: Jump with fist gesture
        if (jumpWithFist && gestureData.isFist)
        {
            shouldJump = true;
        }

        // Execute jump with cooldown
        if (shouldJump && Time.time - lastJumpTime > jumpCooldown)
        {
            ExecuteJump();
            lastJumpTime = Time.time;
        }

        // Debug visualization
        if (showDebugInfo)
        {
            Debug.Log($"🐦 Bird Control - Y:{gestureData.position.y:F2} | Above:{wasAboveThreshold} | Should Jump:{shouldJump}");
        }
    }

    private void ExecuteJump()
    {
        // Call the Jump method via reflection or make it public in Playerpajaro
        // For now, we'll simulate the jump behavior
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(0, player.jumpForce);
            
            if (showDebugInfo)
                Debug.Log("🦅 JUMP!");
        }
    }

    void OnDisable()
    {
        wasAboveThreshold = false;
    }
}
