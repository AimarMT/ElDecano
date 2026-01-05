using System.Collections;
using UnityEngine;
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

    [Header("Teleport")]
    public Transform playerRoot;      // XR Origin
    public Transform teleportTarget;  // Empty con posición/rotación

    [Header("Fade")]
    //public FadeController fadeController;

    private bool isPlayingLocked = false;
    private bool isBusy = false;

    private void Start()
    {
        // Esperamos un frame para que VR se inicialice correctamente
        StartCoroutine(InitializeVR());
    }

    private IEnumerator InitializeVR()
    {
        yield return null; // espera un frame
        // Opcional: fade inicial desde negro si quieres cinematic al principio
        /*if (fadeController != null)
        {
            yield return fadeController.FadeOutCoroutine(); // fade de negro a transparente
        }*/
    }

    public void OnActivate(ActivateEventArgs args)
    {
        if (!GameManager.Instance.irSegundoPiso && !isBusy)
        {
            StartCoroutine(OpenDoorSequence());
        }
        else if (!isPlayingLocked)
        {
            StartCoroutine(PlayLockedSequence());
        }
    }

    private IEnumerator OpenDoorSequence()
    {
        if (teleportTarget == null || playerRoot == null /*|| fadeController == null*/)
        {
            Debug.LogError("ToFloorTwoDoor: Faltan referencias asignadas!");
            yield break;
        }

        isBusy = true;
        openandclose.Play("Opening 1");

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        open = true;

        // --- FADE IN (pantalla negra) ---
        //yield return fadeController.FadeInCoroutine();

        // --- TELEPORT ---
        playerRoot.position = teleportTarget.position;
        playerRoot.rotation = teleportTarget.rotation;

        yield return new WaitForEndOfFrame(); // asegura que la cámara se estabilice

        // --- FADE OUT (pantalla visible) ---
        //yield return fadeController.FadeOutCoroutine();

        openandclose.Play("Closing 1");
        open = false;

        GameManager.Instance.irSegundoPiso = true;
        isBusy = false;
    }

    private IEnumerator PlayLockedSequence()
    {
        isPlayingLocked = true;

        if (sourceLocked == null || lockedFirstClip == null)
        {
            isPlayingLocked = false;
            yield break;
        }

        sourceLocked.PlayOneShot(lockedFirstClip);
        yield return new WaitForSeconds(delayBetweenClips);

        if (lockedSecondClip != null)
        {
            sourceLocked.PlayOneShot(lockedSecondClip);
            yield return new WaitForSeconds(lockedSecondClip.length);
        }

        isPlayingLocked = false;
    }
}
