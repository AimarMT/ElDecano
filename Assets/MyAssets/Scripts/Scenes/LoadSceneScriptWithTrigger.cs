using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScriptWithTrigger : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    [SerializeField] string loadingSceneName = "LoadingScene";
    [SerializeField] LoadingSceneData loadingData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            loadingData.sceneToLoad = sceneToLoad;
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}
