using UnityEngine;

public class RevealFaceOnClick : MonoBehaviour
{
    public Renderer glassRenderer;
    public float fadeDuration = 3f;

    private Material glassMaterial;
    private bool isRevealing = false;

    void Awake()
    {
        Debug.Log("[RevealFace] Awake llamado");

        if (glassRenderer == null)
        {
            Debug.LogError("[RevealFace] glassRenderer NO asignado");
            return;
        }

        glassMaterial = glassRenderer.material;

        Debug.Log("[RevealFace] Material usado: " + glassMaterial.name);

        Color c = glassMaterial.color;
        Debug.Log("[RevealFace] Alpha inicial: " + c.a);

        c.a = 0f;
        glassMaterial.color = c;

        Debug.Log("[RevealFace] Alpha forzado a 0 en Awake");
    }

    public void OnXRClick()
    {
        Debug.Log("[RevealFace] OnXRClick llamado");

        if (isRevealing)
        {
            Debug.Log("[RevealFace] Ya se estaba revelando, se ignora");
            return;
        }

        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        isRevealing = true;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            Color c = glassMaterial.color;
            c.a = alpha;
            glassMaterial.color = c;

            Debug.Log("[RevealFace] Alpha actual: " + alpha);

            yield return null;
        }

        Color finalColor = glassMaterial.color;
        finalColor.a = 1f;
        glassMaterial.color = finalColor;

        Debug.Log("[RevealFace] Fade terminado, alpha = 1");
    }
}
