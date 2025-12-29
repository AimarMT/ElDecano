using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NoteLookAt : MonoBehaviour
{
    [Header("Referencia a la cabeza")]
    public Transform head;                // Main Camera

    [Header("Rotación y suavizado")]
    public float rotationSmooth = 10f;    // suavizado de rotación

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (grab.isSelected && head != null)
        {
            // 1️ Rotación: la nota mira hacia la cabeza
            Vector3 direction = head.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(-direction); // -direction para que la cara visible mire a la cabeza

            // 2️ Suavizado de rotación
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);

            
        }
    }
}

