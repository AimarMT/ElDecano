using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class HandGestureMovement : MonoBehaviour
{
    public float speed = 2.0f;
    private XRHandSubsystem _handSubsystem;
    private CharacterController _characterController;
    private Transform _cameraTransform;

    void Awake()
    {
        // Cacheamos el CharacterController y la cámara para evitar llamadas repetidas en Update
        _characterController = GetComponent<CharacterController>();
        _cameraTransform = Camera.main ? Camera.main.transform : null;

        // Inicializamos el subsistema de manos en Awake (más "bonito" y claro)
        _handSubsystem = GetHandSubsystem();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            MoveForward();
        }

        // Si el subsistema no está disponible o dejó de ejecutarse, intentamos recuperarlo
        if (_handSubsystem == null || !_handSubsystem.running)
        {
            _handSubsystem = GetHandSubsystem();
        }

        if (_handSubsystem != null && _handSubsystem.leftHand.isTracked)
        {
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
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var wrist = hand.GetJoint(XRHandJointID.Wrist);

        if (middleTip.TryGetPose(out Pose tipPose) && wrist.TryGetPose(out Pose wristPose))
        {
            float distance = Vector3.Distance(tipPose.position, wristPose.position);
            return distance < 0.08f;
        }
        return false;
    }

    void MoveForward()
    {
        var cam = _cameraTransform ?? Camera.main?.transform;
        if (cam == null) return;

        Vector3 direction = cam.forward;
        direction.y = 0;

        if (_characterController != null)
        {
            _characterController.Move(direction.normalized * speed * Time.deltaTime);
        }
        else
        {
            // Fallback si no hay CharacterController en el GameObject
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }
}