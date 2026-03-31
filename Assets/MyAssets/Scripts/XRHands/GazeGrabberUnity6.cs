using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class GazeHandGrabberUnity6 : MonoBehaviour
{
    [Header("Configuración de Mirada (Gaze)")]
    public Transform gazeOrigin;
    public float maxGrabDistance = 5.0f;
    public LayerMask grabLayer;

    [Header("Detección de Puño (Hand Tracking)")]
    public Handedness handToTrack = Handedness.Right;
    [Range(0.01f, 0.1f)] public float fistThreshold = 0.08f;

    [Header("Referencias XRI (Unity 6)")]
    public XRInteractionManager interactionManager;

    [Tooltip("Arrastra aquí el objeto que tiene el Interactor (ej. Right Hand Near-Far Interactor)")]
    public GameObject handInteractorObject;

    private IXRSelectInteractor _interactor; // Interfaz genérica para el agarre
    private XRHandSubsystem _handSubsystem;
    private XRGrabInteractable _targetObject;
    private bool _isGazing = false;
    private bool _isGrabbing = false;

    void Awake()
    {
        if (handInteractorObject != null)
        {
            // Buscamos cualquier componente que implemente la interfaz de selección
            _interactor = handInteractorObject.GetComponent<IXRSelectInteractor>();
        }
    }

    void Update()
    {
        // Validaciones
        if (gazeOrigin == null || interactionManager == null || _interactor == null) return;

        if (_handSubsystem == null || !_handSubsystem.running)
        {
            _handSubsystem = GetHandSubsystem();
        }

        if (!_isGrabbing)
        {
            CheckGaze();
        }

        if (_handSubsystem != null)
        {
            bool fistClosed = IsFistClosed();

            if (fistClosed && _isGazing && _targetObject != null && !_isGrabbing)
            {
                TryGrab();
            }
            else if (!fistClosed && _isGrabbing)
            {
                TryRelease();
            }
        }
    }

    void CheckGaze()
    {
        RaycastHit hit;
        if (Physics.Raycast(gazeOrigin.position, gazeOrigin.forward, out hit, maxGrabDistance, grabLayer))
        {
            if (hit.collider.TryGetComponent<XRGrabInteractable>(out var interactable))
            {
                _targetObject = interactable;
                _isGazing = true;
                return;
            }
        }
        _targetObject = null;
        _isGazing = false;
    }

    bool IsFistClosed()
    {
        var hand = (handToTrack == Handedness.Right) ? _handSubsystem.rightHand : _handSubsystem.leftHand;

        if (hand.isTracked)
        {
            var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
            var wrist = hand.GetJoint(XRHandJointID.Wrist);

            if (middleTip.TryGetPose(out Pose tipPose) && wrist.TryGetPose(out Pose wristPose))
            {
                float distance = Vector3.Distance(tipPose.position, wristPose.position);
                return distance < fistThreshold;
            }
        }
        return false;
    }

    void TryGrab()
    {
        if (_targetObject == null) return;

        // Forzamos el inicio de la selección
        interactionManager.SelectEnter(_interactor, (IXRSelectInteractable)_targetObject);
        _isGrabbing = true;
    }

    void TryRelease()
    {
        if (_isGrabbing)
        {
            interactionManager.SelectExit(_interactor, (IXRSelectInteractable)_targetObject);
            _isGrabbing = false;
            _targetObject = null;
        }
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.Count > 0 ? subsystems[0] : null;
    }
}