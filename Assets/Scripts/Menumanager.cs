using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Empezar()
    {
        SceneManager.LoadScene("flapibird");
    }

    public void Salir()
    {
        SceneManager.LoadScene("cuarto principal");
    }
      public void JUEGOpc()
    {
        SceneManager.LoadScene("Windowsescritorio");
    }
      public void Hormiguero()
    {
        SceneManager.LoadScene("hormiguero");
    }
}