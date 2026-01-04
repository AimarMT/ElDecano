using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class puertasUni : MonoBehaviour
{
    [Header("Animación de la puerta")]
    public Animator openandclose;
    public bool open;

    [Header("Sonidos de abrir/cerrar")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Sonidos de puerta cerrada")]
    public AudioSource sourceLocked;
    public AudioClip lockedFirstClip;
    public AudioClip lockedSecondClip;
    public float delayBetweenClips = 0.15f;

    private bool isPlayingLocked = false;

    void Start()
    {
        open = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnActivate(ActivateEventArgs args)
    {
        // Revisar si la nota fue leída
        if (!GameManager.Instance.notaUniLeida)
        {
            if (!isPlayingLocked)
                StartCoroutine(PlayLockedSequence());
            return;
        }

        // Abrir o cerrar normalmente
        if (!open)
            StartCoroutine(opening());
        else
            StartCoroutine(closing());
    }

    IEnumerator opening()
    {
        openandclose.Play("Opening 1");

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        open = true;
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator closing()
    {
        openandclose.Play("Closing 1");

        if (audioSource && closeSound)
            audioSource.PlayOneShot(closeSound);

        open = false;
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator PlayLockedSequence()
    {
        isPlayingLocked = true;

        if (sourceLocked == null || lockedFirstClip == null)
        {
            isPlayingLocked = false;
            yield break;
        }

        // Primer sonido
        sourceLocked.PlayOneShot(lockedFirstClip);

        yield return new WaitForSeconds(delayBetweenClips);

        // Segundo sonido
        if (lockedSecondClip != null)
        {
            sourceLocked.PlayOneShot(lockedSecondClip);
            yield return new WaitForSeconds(lockedSecondClip.length);
        }

        isPlayingLocked = false;
    }
}
