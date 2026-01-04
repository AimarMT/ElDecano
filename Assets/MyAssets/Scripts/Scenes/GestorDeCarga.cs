using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorDeCarga : MonoBehaviour
{
    [SerializeField] LoadingSceneData loadingData;

    void Start()
    {
        StartCoroutine(CargaRealAsincrona());
    }

    IEnumerator CargaRealAsincrona()
    {
        yield return new WaitForSeconds(1.5f);

        if (loadingData == null)
        {
            Debug.LogError("LoadingSceneData no asignado en GestorDeCarga");
            yield break;
        }

        if (string.IsNullOrEmpty(loadingData.sceneToLoad))
        {
            Debug.LogError("sceneToLoad está vacío en LoadingSceneData");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(loadingData.sceneToLoad);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }
    }
}
