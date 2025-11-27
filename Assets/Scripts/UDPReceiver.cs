using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] int port = 5052;
    
    [Header("Controller References")]
    [SerializeField] HandPlaneController planeController;
    // [SerializeField] HandFlappyBirdController flappyController; // Disabled for now
    
    [Header("Debug")]
    [SerializeField] bool showDebugLogs = false;

    Thread receiveThread;
    UdpClient client;
    bool isRunning = true;
    
    public static UDPReceiver instance;
    public bool IsConnected { get; private set; }
    private float lastDataTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log($"🎮 UDPReceiver started on port {port}");
    }

    void Update()
    {
        // Check connection status
        IsConnected = (Time.time - lastDataTime) < 1f;
    }

    void ReceiveData()
    {
        client = new UdpClient(port);
        while (isRunning)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                
                // Expected format: "x,y,isFist,isThumbsUp,isOpenHand" 
                // Example: "0.5,-0.2,1,0,0"
                string[] parts = text.Split(',');
                if (parts.Length >= 5)
                {
                    float x = float.Parse(parts[0]);
                    float y = float.Parse(parts[1]);
                    bool isFist = int.Parse(parts[2]) == 1;
                    bool isThumbsUp = int.Parse(parts[3]) == 1;
                    bool isOpenHand = int.Parse(parts[4]) == 1;

                    HandGestureData gestureData = new HandGestureData(
                        new Vector3(x, y, 0), 
                        isFist, 
                        isThumbsUp, 
                        isOpenHand
                    );

                    lastDataTime = Time.time;

                    // Dispatch to main thread
                    MainThreadDispatcher.Enqueue(() => {
                        if (showDebugLogs)
                            Debug.Log($"📡 Received: {gestureData}");
                            
                        // Send to active controller
                        if (planeController != null && planeController.enabled)
                            planeController.UpdateHandControl(gestureData);
                            
                        // if (flappyController != null && flappyController.enabled)
                        //     flappyController.UpdateHandControl(gestureData);
                    });
                }
                else if (parts.Length == 3) // Legacy format support
                {
                    float x = float.Parse(parts[0]);
                    float y = float.Parse(parts[1]);
                    bool isFist = int.Parse(parts[2]) == 1;

                    HandGestureData gestureData = new HandGestureData(
                        new Vector3(x, y, 0), 
                        isFist, 
                        false, 
                        false
                    );

                    lastDataTime = Time.time;

                    MainThreadDispatcher.Enqueue(() => {
                        if (planeController != null && planeController.enabled)
                            planeController.UpdateHandControl(gestureData);
                            
                        // if (flappyController != null && flappyController.enabled)
                        //     flappyController.UpdateHandControl(gestureData);
                    });
                }
            }
            catch (System.Exception e)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"UDP Error: {e.Message}");
            }
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        if (client != null) client.Close();
        if (receiveThread != null) receiveThread.Abort();
        Debug.Log("🛑 UDPReceiver stopped");
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}

// Simple helper to run code on main thread
public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly System.Collections.Generic.Queue<System.Action> _executionQueue = new System.Collections.Generic.Queue<System.Action>();

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(System.Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
    
    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        if (FindObjectOfType<MainThreadDispatcher>() == null)
        {
            GameObject go = new GameObject("MainThreadDispatcher");
            go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }
}
