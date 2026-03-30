using UnityEngine;
using UnityEngine.XR.Hands; // Necesario para Hand Tracking nativo
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GazeHandGrabberUnity6 : MonoBehaviour
{
    [Header("Configuración de Mirada (Gaze)")]
    [Tooltip("Asigna la 'Main Camera' de tu XR Rig.")]
    public Transform gazeOrigin; 
    public float maxGrabDistance = 5.0f;

    [Header("Detección de Puño (Hand Tracking)")]
    public Handedness handToTrack = Handedness.Left; // ¿Mano izquierda o derecha?
    [Range(0.01f, 0.1f)] public float fistThreshold = 0.04f; // Sensibilidad del cierre

    [Header("Referencias XRI (Unity 6)")]
    [Tooltip("Asigna el 'XR Interaction Manager' de tu escena.")]
    public XRInteractionManager interactionManager;
    
    // NUEVO: No necesitamos arrastrar un Direct Interactor. 
    // Usaremos este transform como la "palma" donde se pegará el objeto.
    [Tooltip("Asigna el propio transform del 'Left Controller' aquí.")]
    public Transform handPalmTransform; 

    private XRGrabInteractable targetObject;
    private bool isGazing = false;
    private XRHandSubsystem handSubsystem;
    private bool grabbingProcessStarted = false;

    void Update()
    {
        // Validaciones de seguridad para Unity 6
        if (gazeOrigin == null || interactionManager == null || handPalmTransform == null) return;

        CheckGaze();

        // Lógica de Gesto: Si el puño está cerrado Y estamos mirando algo válido
        if (IsFistClosed() && isGazing && targetObject != null && !grabbingProcessStarted)
        {
            TryGrab();
        }
        else if (!IsFistClosed() && grabbingProcessStarted)
        {
            // Resetear estado si abrimos la mano
            grabbingProcessStarted = false;
        }
    }

    void CheckGaze()
    {
        RaycastHit hit;
        // Usamos la posición GLOBAL de la cámara (gazeOrigin)
        if (Physics.Raycast(gazeOrigin.position, gazeOrigin.forward, out hit, maxGrabDistance))
        {
            if (hit.collider.TryGetComponent<XRGrabInteractable>(out var interactable))
            {
                targetObject = interactable;
                isGazing = true;
                Debug.DrawRay(gazeOrigin.position, gazeOrigin.forward * hit.distance, Color.green);
                return;
            }
        }
        targetObject = null;
        isGazing = false;
        Debug.DrawRay(gazeOrigin.position, gazeOrigin.forward * maxGrabDistance, Color.red);
    }

    bool IsFistClosed()
    {
        // Buscamos el sistema de manos nativo si no lo tenemos
        if (handSubsystem == null)
        {
            var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
        }

        if (handSubsystem != null)
        {
            var hand = handToTrack == Handedness.Left ? handSubsystem.leftHand : handSubsystem.rightHand;
            
            if (hand.isTracked)
            {
                // Obtenemos la posición de la punta del índice y la palma
                var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
                var palm = hand.GetJoint(XRHandJointID.Palm);

                if (indexTip.TryGetPose(out Pose indexPose) && palm.TryGetPose(out Pose palmPose))
                {
                    // Si el índice está muy cerca de la palma, consideramos que es un puño
                    float distance = Vector3.Distance(indexPose.position, palmPose.position);
                    return distance < fistThreshold;
                }
            }
        }
        return false;
    }

    void TryGrab()
    {
        grabbingProcessStarted = true;

        // Validamos que el objeto no esté seleccionado por otro interactor
        if (targetObject.interactorsSelecting.Count == 0)
        {
            // NUEVO: En Unity 6, si no hay interactor físico, forzamos al Interaction Manager 
            // a 'pegar' el interactable a un interactor lógico o a su transform local.
            // Para un agarre a distancia 'mágico', simplemente moveremos el objeto
            // y luego activaremos la selección.
            
            // 1. Desactivamos físicas temporalmente
            if (targetObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 2. Movemos el objeto a la palma de la mano de forma instantánea
            targetObject.transform.position = handPalmTransform.position;
            targetObject.transform.rotation = handPalmTransform.rotation;

            // 3. Forzamos al Interaction Manager a iniciar la interacción manual
            // Como no tenemos Direct Interactor, el sistema usará un interactor genérico del Manager
            interactionManager.SelectEnter((IXRSelectInteractor)this, (IXRSelectInteractable)targetObject);
        }
    }
}