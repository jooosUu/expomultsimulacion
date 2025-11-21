using UnityEngine;

public class ColonyManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject antPrefab;
    public GameObject leafPrefab;
    public GameObject tunnelPrefab;
    
    [Header("Configuración del Hormiguero")]
    public int initialAntCount = 10;
    public float spawnRadius = 5f;
    
    [Header("Configuración de Túneles")]
    public int tunnelSegments = 15;
    public float tunnelWidth = 0.5f;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Crear túneles del hormiguero
        CreateTunnelSystem();
        
        // Crear hormigas iniciales
        SpawnAnts();
    }
    
    void Update()
    {
        // Clic izquierdo para soltar hojas
        if (Input.GetMouseButtonDown(0))
        {
            DropLeaf();
        }
        
        // Tecla ESPACIO para crear más hormigas
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSingleAnt(Vector2.zero);
        }
    }
    
    void CreateTunnelSystem()
    {
        if (tunnelPrefab == null) return;
        
        // Crear túneles en forma de red
        Vector2 startPos = new Vector2(0, 3);
        
        for (int i = 0; i < tunnelSegments; i++)
        {
            float angle = Random.Range(-45f, 45f);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
            float length = Random.Range(1f, 2.5f);
            
            GameObject tunnel = Instantiate(tunnelPrefab, transform);
            tunnel.transform.position = startPos;
            
            // Ajustar escala del túnel
            tunnel.transform.localScale = new Vector3(tunnelWidth, length, 1);
            tunnel.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
            
            startPos += direction * length;
            
            // Ramificaciones aleatorias
            if (Random.value > 0.6f && i < tunnelSegments - 3)
            {
                CreateBranch(startPos, i);
            }
        }
    }
    
    void CreateBranch(Vector2 startPos, int depth)
    {
        if (tunnelPrefab == null || depth >= tunnelSegments - 2) return;
        
        float angle = Random.value > 0.5f ? Random.Range(30f, 60f) : Random.Range(-60f, -30f);
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
        float length = Random.Range(1f, 2f);
        
        GameObject tunnel = Instantiate(tunnelPrefab, transform);
        tunnel.transform.position = startPos;
        tunnel.transform.localScale = new Vector3(tunnelWidth * 0.8f, length, 1);
        tunnel.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
    }
    
    void SpawnAnts()
    {
        for (int i = 0; i < initialAntCount; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            SpawnSingleAnt(randomPos);
        }
    }
    
    void SpawnSingleAnt(Vector2 position)
    {
        if (antPrefab != null)
        {
            GameObject ant = Instantiate(antPrefab, position, Quaternion.identity, transform);
        }
    }
    
    void DropLeaf()
    {
        if (leafPrefab == null) return;
        
        // Obtener posición del mouse en el mundo
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Crear hoja en esa posición
        GameObject leaf = Instantiate(leafPrefab, mousePos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        
        // Añadir pequeña animación de caída
        Rigidbody2D rb = leaf.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0.5f;
            rb.linearVelocity = Vector2.down * 2f;
        }
    }
}