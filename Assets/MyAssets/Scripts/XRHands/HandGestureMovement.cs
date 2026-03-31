using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class HandGestureMovement : MonoBehaviour
{
    public float speed = 2.0f;
    private XRHandSubsystem _handSubsystem;

    void Update()
    {
        // 1. Buscamos el subsistema si aún no lo tenemos
        if (_handSubsystem == null || !_handSubsystem.running)
        {
            _handSubsystem = GetHandSubsystem();
        }

        if (_handSubsystem != null && _handSubsystem.leftHand.isTracked)
        {
            // 2. Comprobar si el puño está cerrado
            if (IsFist(_handSubsystem.leftHand))
            {
                MoveForward();
            }
        }
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.Count > 0 ? subsystems[0] : null;
    }

    bool IsFist(XRHand hand)
    {
        // Obtenemos la articulación de la punta del dedo corazón y la muñeca
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var wrist = hand.GetJoint(XRHandJointID.Wrist);

        if (middleTip.TryGetPose(out Pose tipPose) && wrist.TryGetPose(out Pose wristPose))
        {
            // Si la punta del dedo está muy cerca de la muñeca, es un puño
            float distance = Vector3.Distance(tipPose.position, wristPose.position);
            return distance < 0.08f; // Ajusta este valor (0.08m = 8cm) según necesites
        }
        return false;
    }

    void MoveForward()
    {
        Vector3 direction = Camera.main.transform.forward;
        direction.y = 0;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}