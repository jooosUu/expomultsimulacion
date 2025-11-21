using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public string sceneName = "Hormiguero"; // Nombre de la escena a cargar
    public KeyCode interactionKey = KeyCode.E; // Tecla de interacción
    
    [Header("UI (Opcional)")]
    public GameObject interactionPrompt; // Texto "Presiona E" (opcional)
    
    private bool playerInRange = false;

    void Start()
    {
        // Ocultar el prompt al inicio si existe
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Si el jugador está en rango y presiona E
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            LoadScene();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar cuando el jugador entra en el área
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Mostrar prompt si existe
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            
            Debug.Log("Presiona " + interactionKey.ToString() + " para interactuar");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Detectar cuando el jugador sale del área
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Ocultar prompt si existe
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    void LoadScene()
    {
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // Visualizar el área de interacción en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}