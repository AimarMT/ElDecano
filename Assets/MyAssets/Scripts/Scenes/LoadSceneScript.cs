using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScript : MonoBehaviour
{
    public void CargarPrimeraCinematica()
    {
        
        SceneManager.LoadScene("PrimeraCinematica");
        Debug.Log("Cargando PrimeraCinematica");
    }

     public void CargarMainMenu()
    {
        
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Cargando MainMenu");
    }

    public void ExitAPK()
    {
        Application.Quit();
        Debug.Log("Saliendo de la APK");
    }
}
