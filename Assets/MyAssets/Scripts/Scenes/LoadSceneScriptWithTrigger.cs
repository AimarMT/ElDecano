using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoaderManager : MonoBehaviour
{
    [SerializeField] string sceneToLoad = "UniversityMap";
    [SerializeField] string loadingSceneName = "LoadingScene";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        // Solo saltamos al loading. El "GestorDeCarga" de la otra escena hará el resto.
        SceneManager.LoadScene("LoadingScene");
    }
}

    IEnumerator LoadSequence()
{
    // 1. Carga el Loading
    SceneManager.LoadScene(loadingSceneName);
    yield return null; 

    // 2. Carga la Universidad en el fondo
    AsyncOperation finalOp = SceneManager.LoadSceneAsync(sceneToLoad);
    
    // IMPORTANTE: Asegúrate de que esto sea true para que cambie sola al terminar
    finalOp.allowSceneActivation = true; 

    while (!finalOp.isDone)
    {
        // Si el progreso llega a 0.9, Unity ya terminó de cargar
        if (finalOp.progress >= 0.9f)
        {
            Debug.Log("Carga completa, activando escena...");
        }
        yield return null;
    }
}
}