using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class HandGrabByFist : MonoBehaviour
{
    private XRDirectInteractor _interactor;
    private XRHandSubsystem _handSubsystem;

    [Header("Configuracion")]
    public bool isLeftHand = true;
    public float fistThreshold = 0.08f;

    private bool _isGrabbing = false;

    void Awake() => _interactor = GetComponent<XRDirectInteractor>();

    void Update()
    {
        Debug.Log("Hovered: " + _interactor.interactablesHovered.Count);
        if (_handSubsystem == null || !_handSubsystem.running)
            _handSubsystem = GetHandSubsystem();

        if (_handSubsystem != null)
        {
            var hand = isLeftHand ? _handSubsystem.leftHand : _handSubsystem.rightHand;

            if (hand.isTracked)
            {
                UpdateInteractorPose(hand);

                bool fistDetected = CheckFist(hand);

                if (fistDetected && !_isGrabbing)
                {
                    _isGrabbing = true;
                    OnGrabStart();
                }
                else if (!fistDetected && _isGrabbing)
                {
                    _isGrabbing = false;
                    OnGrabEnd();
                }
            }
        }
    }

    private void OnGrabStart()
    {
        if (_interactor.interactablesHovered.Count > 0 && !_interactor.hasSelection)
        {
            var target = _interactor.interactablesHovered[0] as IXRSelectInteractable;
            if (target != null)
            {
                _interactor.StartManualInteraction(target);
                Debug.Log("[Fist] Agarrando");
            }
        }
    }

    private void OnGrabEnd()
    {
        if (_interactor.hasSelection)
        {
            _interactor.EndManualInteraction();
            Debug.Log("[Fist] Soltado");
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

    void UpdateInteractorPose(XRHand hand)
    {
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (indexTip.TryGetPose(out Pose pose))
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.Count > 0 ? subsystems[0] : null;
    }
}