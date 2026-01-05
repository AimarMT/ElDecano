using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [Header("Imagen de Fade")]
    public Image fadeImage;

    [Header("Duración del fade (segundos)")]
    public float fadeDuration = 2f;

    private Coroutine currentFade;

    void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: No se ha asignado la Image del fade.");
            return;
        }

        // Empieza desactivado y alpha 0 para evitar glitches VR
        fadeImage.gameObject.SetActive(false);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
    }

  
    public void StartFade(float fromAlpha, float toAlpha)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(fromAlpha, toAlpha));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha)
    {
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, toAlpha);

        if (toAlpha == 0f)
            fadeImage.gameObject.SetActive(false);

        currentFade = null;
    }

    public void FadeOut() => StartFade(1f, 0f);
    public void FadeIn() => StartFade(0f, 1f);

    public IEnumerator FadeOutCoroutine()
    {
        FadeOut();
        while (currentFade != null) yield return null;
    }

    public IEnumerator FadeInCoroutine()
    {
        FadeIn();
        while (currentFade != null) yield return null;
    }
}
