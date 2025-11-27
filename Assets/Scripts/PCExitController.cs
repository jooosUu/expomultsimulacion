using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PCExitController : MonoBehaviour
{
    [Header("Configuración")]
    public string roomSceneName = "Cuarto"; // Nombre de la escena del cuarto

    void Update()
    {
        // Detectar botón B (Este) en Gamepad o Escape en teclado
        bool exitInput = false;

        if (Gamepad.current != null)
        {
            exitInput = Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitInput = true;
        }

        if (exitInput)
        {
            Debug.Log("Saliendo del PC...");
            SceneManager.LoadScene(roomSceneName);
        }
    }
}
