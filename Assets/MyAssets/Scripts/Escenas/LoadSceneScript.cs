using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScript : MonoBehaviour
{
    public void LoadScene()
    {
        
        SceneManager.LoadScene("PrimeraCinematica");
        Debug.Log("Cargando PrimeraCinematica");
    }

    public void ExitAPK()
    {
        Application.Quit();
        Debug.Log("Saliendo de la APK");
    }
}
