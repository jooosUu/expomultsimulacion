using UnityEngine;

public class PanelManager : MonoBehaviour
{
    // Método para abrir un panel específico
    public void OpenPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    // Método para cerrar un panel específico
    public void ClosePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Método para alternar (toggle) un panel
    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    // Método para cerrar todos los paneles de una lista
    public void CloseAllPanels(GameObject[] panels)
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}