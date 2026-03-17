using UnityEngine;
using UnityEngine.XR.Hands;

using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class HandGrabByFist : MonoBehaviour
{
    private XRDirectInteractor _interactor;
    private XRHandSubsystem _handSubsystem;
    
    [Header("Configuracion")]
    public bool isLeftHand = true;
    public float fistThreshold = 0.08f;

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
                bool isFist = CheckFist(hand);
                
                // Unity 6 usa 'hasSelection' en lugar de 'hasInteractablesSelected'
                if (isFist && !_interactor.hasSelection)
                {
                    TryGrab();
                }
                else if (!isFist && _interactor.hasSelection)
                {
                    _interactor.EndManualInteraction();
                }
            }
        }
    }

    private void TryGrab()
    {
        // Unity 6: comprobamos si hay algo cerca para agarrar
        if (_interactor.hasHover)
        {
            // Accedemos al primer objeto que estamos tocando
            var interactable = _interactor.interactablesHovered[0];
            _interactor.StartManualInteraction((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)interactable);
        }
    }

    bool CheckFist(XRHand hand)
    {
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var wrist = hand.GetJoint(XRHandJointID.Wrist);
        if (middleTip.TryGetPose(out Pose tipPose) && wrist.TryGetPose(out Pose wristPose))
        {
            return Vector3.Distance(tipPose.position, wristPose.position) < fistThreshold;
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