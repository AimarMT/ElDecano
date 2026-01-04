using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ToFloorTwoDoor : MonoBehaviour
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
    public float delayBetweenClips = 0.15f;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    [Header("Teleport")]
    public Transform playerRoot; // XR Origin
    public Vector3 teleportPosition;
    public Vector3 teleportRotation;

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
        // Revisar si la nota fue leída
        if (!GameManager.Instance.irSegundoPiso && !isBusy)
        {
            StartCoroutine(fullMovement());
        }

        if (!isPlayingLocked)
            StartCoroutine(PlayLockedSequence());
        return;
    }

    IEnumerator fullMovement()
    {
        isBusy = true;
        openandclose.Play("Opening 1");

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);
        open = true;


        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = t / fadeDuration;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        playerRoot.position = teleportPosition;
        playerRoot.eulerAngles = teleportRotation;

        yield return new WaitForSeconds(0.1f);


        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = 1f - (t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
        openandclose.Play("Closing 1");
        open = false;

        GameManager.Instance.irSegundoPiso = true;
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
