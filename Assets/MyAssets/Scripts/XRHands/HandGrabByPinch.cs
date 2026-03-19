using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class HandGrabByPinch : MonoBehaviour
{
    private XRDirectInteractor _interactor;
    private XRHandSubsystem _handSubsystem;
    
    [Header("Configuración de Mano")]
    public bool isLeftHand = true;

    [Header("Ajustes de Pellizco")]
    [Tooltip("Distancia en metros para activar el pellizco.")]
    public float pinchDistanceThreshold = 0.03f; 
    
    private bool _isCurrentlyPinching = false;

    void Awake() 
    {
        _interactor = GetComponent<XRDirectInteractor>();
    }

    void Update()
    {
        if (_handSubsystem == null || !_handSubsystem.running)
            _handSubsystem = GetHandSubsystem();

        if (_handSubsystem != null)
        {
            var hand = isLeftHand ? _handSubsystem.leftHand : _handSubsystem.rightHand;

            if (hand.isTracked)
            {
                bool pinchDetected = CheckPinch(hand);
                
                // Cambio de estado: Empezar a pellizcar
                if (pinchDetected && !_isCurrentlyPinching)
                {
                    _isCurrentlyPinching = true;
                    OnPinchStart();
                }
                // Cambio de estado: Soltar el pellizco
                else if (!pinchDetected && _isCurrentlyPinching)
                {
                    _isCurrentlyPinching = false;
                    OnPinchEnd();
                }
            }
        }
    }

    private void OnPinchStart()
    {
        // Solo intentamos agarrar si tenemos algo resaltado (Hover) y no tenemos nada seleccionado ya
        if (_interactor.hasHover && !_interactor.hasSelection)
        {
            // CORRECCIÓN: Añadimos (IXRSelectInteractable) antes del objeto para hacer el cast explícito
            IXRSelectInteractable target = (IXRSelectInteractable)_interactor.interactablesHovered[0];
        
            // Iniciamos la interacción manual
            _interactor.StartManualInteraction(target);
            Debug.Log($"[Pinch] Agarrando con éxito: {target.transform.name}");
        }
    }

    private void OnPinchEnd()
    {
        if (_interactor.hasSelection)
        {
            _interactor.EndManualInteraction();
            Debug.Log("[Pinch] Objeto soltado");
        }
    }

    bool CheckPinch(XRHand hand)
    {
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumbTip.TryGetPose(out Pose thumbPose) && indexTip.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            return distance < pinchDistanceThreshold;
        }
        return false;
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        foreach (var s in subsystems)
        {
            if (s.running) return s;
        }
        return null;
    }
}