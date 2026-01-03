using System.Collections;
using UnityEngine;

public class TriggerDetenerMusica : MonoBehaviour
{
    private bool triggered = false;
    public float stoppingTime = 0.3f;
    public AudioSource musicSource;
    public AudioClip sfxClip;        // efecto al detener música
    public AudioSource sfxSource;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(FadeOutMusic(stoppingTime));
        }
    }

    IEnumerator FadeOutMusic(float duration)
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
            musicSource.volume = startVolume; // restauramos volumen original por si se vuelve a usar

            // Reproducir efecto al detener música
            if (sfxSource != null && sfxClip != null)
                sfxSource.PlayOneShot(sfxClip);
        }
    }
}
