using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ToFloorOneDoor : MonoBehaviour
{
    [Header("Animación de la puerta")]
    public Animator openandclose;
    public bool open;


    public GameObject decanoPiso2;

    [Header("Sonidos de abrir/cerrar")]
    public AudioSource audioSource;
    public AudioClip openSound;

    [Header("Sonidos de puerta cerrada")]
    public AudioSource sourceLocked;
    public AudioClip lockedFirstClip;
    public AudioClip lockedSecondClip;
    public AudioClip lockedSecondClipNoteGrabbed;
    public float delayBetweenClips = 0.15f;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    [Header("Teleport")]
    public Transform playerRoot;      // XR Origin
    public Transform teleportPoint;   // Empty de destino

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
        if (!GameManager.Instance.primerMinijuegoCompletado)
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

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        open = true;

        // Teletransporte al Empty (posición y rotación en mundo)
        playerRoot.position = teleportPoint.position;
        playerRoot.rotation = teleportPoint.rotation;

        yield return new WaitForSeconds(0.3f);

        openandclose.Play("Closing 1");
        open = false;

        GameManager.Instance.irSegundoPiso = true;
        decanoPiso2.SetActive(true);

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

        if (GameManager.Instance.notaUniLeida)
        {
            sourceLocked.PlayOneShot(lockedSecondClipNoteGrabbed);
            yield return new WaitForSeconds(lockedSecondClipNoteGrabbed.length);
        }
        else if (lockedSecondClip != null)
        {
            sourceLocked.PlayOneShot(lockedSecondClip);
            yield return new WaitForSeconds(lockedSecondClip.length);
        }

        isPlayingLocked = false;
    }
}

