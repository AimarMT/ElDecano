using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NoteLookAt : MonoBehaviour
{
    [Header("Referencia a la cabeza")]
    public Transform head;

    [Header("Rotación y suavizado")]
    public float rotationSmooth = 10f;

    private XRGrabInteractable grab;
    private bool isGrabbed = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // Eventos XR
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        // Limpieza (buena práctica)
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed || head == null)
            return;

        // La nota mira hacia la cabeza SOLO cuando está agarrada
        Vector3 direction = head.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(-direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }
}
