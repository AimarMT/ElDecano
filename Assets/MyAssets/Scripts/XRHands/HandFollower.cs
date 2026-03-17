using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class HandFollower : MonoBehaviour
{
    public bool isLeftHand = true;
    private XRHandSubsystem _handSubsystem;

    void Update()
    {
        if (_handSubsystem == null || !_handSubsystem.running){
            _handSubsystem = GetHandSubsystem();
        }
            

        if (_handSubsystem != null)
        {
            var hand = isLeftHand ? _handSubsystem.leftHand : _handSubsystem.rightHand;
            
            // Si la mano está siendo rastreada, movemos el controlador a su posición
            if (hand.isTracked)
            {
                var wrist = hand.GetJoint(XRHandJointID.Wrist);
                if (wrist.TryGetPose(out Pose wristPose))
                {
                    transform.SetPositionAndRotation(wristPose.position, wristPose.rotation);
                }
            }
        }
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.Count > 0 ? subsystems[0] : null;
    }
}