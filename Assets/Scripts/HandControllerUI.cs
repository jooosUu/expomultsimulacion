using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandControllerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject uiPanel;
    [SerializeField] TextMeshProUGUI connectionStatusText;
    [SerializeField] TextMeshProUGUI gestureText;
    [SerializeField] Image connectionIndicator;
    
    [Header("Colors")]
    [SerializeField] Color connectedColor = Color.green;
    [SerializeField] Color disconnectedColor = Color.red;

    [Header("Settings")]
    [SerializeField] bool showUI = true;
    [SerializeField] KeyCode toggleKey = KeyCode.F1;

    private HandGestureData lastGesture;

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(showUI);
    }

    void Update()
    {
        // Toggle UI
        if (Input.GetKeyDown(toggleKey))
        {
            showUI = !showUI;
            if (uiPanel != null)
                uiPanel.SetActive(showUI);
        }

        if (!showUI) return;

        // Update connection status
        bool isConnected = UDPReceiver.instance != null && UDPReceiver.instance.IsConnected;
        
        if (connectionStatusText != null)
        {
            connectionStatusText.text = isConnected ? "✅ Conectado" : "❌ Desconectado";
            connectionStatusText.color = isConnected ? connectedColor : disconnectedColor;
        }

        if (connectionIndicator != null)
        {
            connectionIndicator.color = isConnected ? connectedColor : disconnectedColor;
        }

        // Show instructions when disconnected
        if (gestureText != null && !isConnected)
        {
            gestureText.text = "Ejecuta: python Assets/Scripts/hand_tracking.py";
        }
    }

    public void UpdateGestureDisplay(HandGestureData gesture)
    {
        lastGesture = gesture;
        if (gestureText != null && showUI)
        {
            string gestureString = "Gesto: ";
            
            if (gesture.isThumbsUp)
                gestureString += "👍 PULGAR ARRIBA";
            else if (gesture.isFist)
                gestureString += "✊ PUÑO";
            else if (gesture.isOpenHand)
                gestureString += "✋ MANO ABIERTA";
            else
                gestureString += "✋ Normal";

            gestureString += $"\nPos: ({gesture.position.x:F2}, {gesture.position.y:F2})";
            gestureText.text = gestureString;
        }
    }
}
