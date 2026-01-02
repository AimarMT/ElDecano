using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class PlaySoundOnGroundHit : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip impactSound;

    [Header("Impacto mínimo para sonar")]
    public float minImpactForce = 0.5f;
    private XRGrabInteractable grab;
    private bool physicsReady = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Esperar un frame para que la física se estabilice
        StartCoroutine(EnablePhysicsNextFrame());
    }

    System.Collections.IEnumerator EnablePhysicsNextFrame()
    {
        yield return null; // esperar 1 frame
        physicsReady = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1️ Evitar sonido al iniciar el juego
        if (!physicsReady)
            return;

        // 2️ No sonar si está siendo grabbeado
        if (grab != null && grab.isSelected)
            return;

        // 3️ Evitar micro-golpes
        if (collision.relativeVelocity.magnitude < minImpactForce)
            return;

        // 4️ Reproducir sonido
        if (audioSource && impactSound)
        {
            audioSource.PlayOneShot(impactSound);
        }
    }
}
