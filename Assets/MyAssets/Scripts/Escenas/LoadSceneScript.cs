using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScript : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene("PrimeraCinematica");
    }

    public void ExitAPK()
    {
        Application.Quit();
    }
}
