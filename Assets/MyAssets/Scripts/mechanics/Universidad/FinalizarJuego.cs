using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class FinalizarJuego : MonoBehaviour
{
    [Header("Animación de la puerta")]
    public Animator openandclose;
    public bool open;

    [Header("Sonidos de abrir/cerrar")]
    public AudioSource audioSource;
    public AudioClip openSound;

    [Header("Sonidos de puerta cerrada")]
    public AudioSource sourceLocked;
    public AudioClip lockedFirstClip;
    public AudioClip lockedSecondClip;
    public AudioClip lockedSecondClipNoteGrabbed;
    public float delayBetweenClips = 0.15f;


    private bool isPlayingLocked = false;
    private bool isBusy = false;

    void Start()
    {
        open = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnActivate(ActivateEventArgs args)
    {
        if (!GameManager.Instance.segundoMinijuegoCompletado)
        {
            if (!isPlayingLocked)
                StartCoroutine(PlayLockedSequence());
            return;
        }

        if (!isBusy)
        {
            StartCoroutine(fullMovement());
        }
    }

    IEnumerator fullMovement()
    {
        isBusy = true;
        openandclose.Play("Opening 1");
        audioSource.PlayOneShot(openSound);

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("WinMenu");

        isBusy = false;
    }

    IEnumerator PlayLockedSequence()
    {
        isPlayingLocked = true;

        if (sourceLocked == null || lockedFirstClip == null)
        {
            isPlayingLocked = false;
            yield break;
        }

        sourceLocked.PlayOneShot(lockedFirstClip);
        yield return new WaitForSeconds(delayBetweenClips);

        
        
            sourceLocked.PlayOneShot(lockedSecondClip);
            yield return new WaitForSeconds(lockedSecondClip.length);
        

        isPlayingLocked = false;
    }
}
