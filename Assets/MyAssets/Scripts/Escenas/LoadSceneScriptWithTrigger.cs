using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneScriptWithTrigger : MonoBehaviour
{
   [SerializeField] string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
