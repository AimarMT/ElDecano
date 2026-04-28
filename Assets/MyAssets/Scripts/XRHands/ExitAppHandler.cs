using UnityEngine;
using UnityEngine.InputSystem;

public class ExitAppHandler : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Saliendo de la aplicación desde la muñeca...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}