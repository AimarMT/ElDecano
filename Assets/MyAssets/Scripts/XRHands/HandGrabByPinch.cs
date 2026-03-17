using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class HandGrabByPinch : MonoBehaviour
{
    private XRDirectInteractor _interactor;
    private XRHandSubsystem _handSubsystem;
    public bool isLeftHand = true;

    [Header("Ajustes de Pellizco")]
    public float pinchDistanceThreshold = 0.07f; // Distancia para activar (2cm)

    void Awake() => _interactor = GetComponent<XRDirectInteractor>();

    void Update()
    {
        if (_handSubsystem == null || !_handSubsystem.running)
            _handSubsystem = GetHandSubsystem();

        if (_handSubsystem != null)
        {
            var hand = isLeftHand ? _handSubsystem.leftHand : _handSubsystem.rightHand;

            if (hand.isTracked)
            {
                bool isPinching = CheckPinch(hand);
                
                if (isPinching && !_interactor.hasSelection)
                {
                    if (_interactor.hasHover) 
                        _interactor.StartManualInteraction((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)_interactor.interactablesHovered[0]);
                }
                else if (!isPinching && _interactor.hasSelection)
                {
                    _interactor.EndManualInteraction();
                }
            }
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
        return subsystems.Count > 0 ? subsystems[0] : null;
    }
}