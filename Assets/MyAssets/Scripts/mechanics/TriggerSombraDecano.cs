using System.Collections;
using UnityEngine;

public class TriggerSombraDecano : MonoBehaviour
{
    public MovimientoSombra sombra;

    [Header("Efecto")]
    public AudioSource sfxSource;
    public AudioClip sfxClip;

    [Header("Música de fondo")]
    public AudioSource musicSource;
    public AudioClip musicClip;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || triggered) return;

        if (GameManager.Instance.notaLeida)
        {
            triggered = true;

            // Efecto puntual
            sfxSource.PlayOneShot(sfxClip);

            // Movimiento de la sombra
            sombra.StartShadowMovement();

            // Música de fondo con loop
            StartCoroutine(ReproducirMusicaConDelay());
        }
    }

    IEnumerator ReproducirMusicaConDelay()
    {
        yield return new WaitForSeconds(0.5f); // Delay opcional

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }
}
