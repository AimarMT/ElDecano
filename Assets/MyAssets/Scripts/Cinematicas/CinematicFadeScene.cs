using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CinematicFadeScene : MonoBehaviour
{
    [Header("CanvasGroup del fade")]
    public CanvasGroup fadeCanvas;

    [Header("Duracion del fade")]
    public float fadeDuration = 2f;

    [Header("Escena a cargar")]
    public string sceneToLoad;

    private bool isFading = false;

    public void StartFade()
    {
        if (isFading) return;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        isFading = true;

        float t = 0f;
        fadeCanvas.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t / fadeDuration;
            yield return null;
        }

        fadeCanvas.alpha = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }
}
