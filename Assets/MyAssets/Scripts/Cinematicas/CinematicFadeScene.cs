using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CinematicFadeScene : MonoBehaviour
{
    [Header("CanvasGroup del fade")]
    public CanvasGroup fadeCanvas;

    [Header("Duración del fade")]
    public float fadeDuration = 2f;

    [Header("Escena final a cargar")]
    public string finalSceneToLoad;

    [Header("ScriptableObject de carga")]
    public LoadingSceneData loadingData;

    [Header("Nombre de la escena de carga")]
    public string loadingSceneName = "LoadingScene";

    private bool isFading = false;

    public void StartFade()
    {
        if (isFading) return;
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        isFading = true;

        float t = 0f;
        fadeCanvas.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;

        
        if (loadingData == null)
        {
            Debug.LogError("LoadingSceneData no asignado en CinematicFadeScene");
            yield break;
        }

        loadingData.sceneToLoad = finalSceneToLoad;

        
        SceneManager.LoadScene(loadingSceneName);
    }
}
