using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GazeAudioTriggerAndoni : MonoBehaviour
{
    [Header("Gaze Settings")]
    public float gazeTime = 5f;

    private float gazeTimer = 0f;
    private bool isGazing = false;
    private bool hasPlayed = false;

    private AudioSource audioSource;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }

        interactable.hoverEntered.AddListener(OnGazeEnter);
        interactable.hoverExited.AddListener(OnGazeExit);
    }

    void Update()
    {
        if (!isGazing || hasPlayed) return;

        gazeTimer += Time.deltaTime;

        if (gazeTimer >= gazeTime)
        {
            PlayAudio();
        }
    }

    void OnGazeEnter(HoverEnterEventArgs args)
    {
        isGazing = true;
        gazeTimer = 0f;
    }

    void OnGazeExit(HoverExitEventArgs args)
    {
        isGazing = false;
        gazeTimer = 0f;
    }

    void PlayAudio()
    {
        hasPlayed = true;
        audioSource.Play();
        Debug.Log("Audio reproducido por Gaze XR");
    }
}
