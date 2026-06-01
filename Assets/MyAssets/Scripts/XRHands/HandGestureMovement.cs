using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using Unity.Netcode;

[DefaultExecutionOrder(10000)]
public class HandGestureMovement : MonoBehaviour
{
    public float speed = 2.0f;

    private XRHandSubsystem _handSubsystem;
    private Transform _cameraTransform;
    private float _verticalVelocity = 0f;
    private bool _activated = false;
    private int _debugFrames = 0;
    private const float Gravity = -9.81f;

    // Anchored floor Y set once after grounding in Activate().
    // OpenXR native plugin writes Y = tracking_floor_offset to the CC's
    // physics state on every cc.Move call, dropping the player ~50 m.
    // By moving via transform.position and clamping to _floorY we bypass that.
    private float _floorY = float.NaN;

    private bool _prevWantsMove = false;
    private bool _prevGrounded  = false;

    void Awake()
    {
        var xrRefs = GetComponent<XRRigReferences2>();
        if (xrRefs != null && xrRefs.head != null)
            _cameraTransform = xrRefs.head;
        else
        {
            _cameraTransform = Camera.main ? Camera.main.transform : null;
            Debug.LogWarning("[HGM] XRRigReferences2 no encontrado, usando Camera.main");
        }

        _handSubsystem = GetHandSubsystem();
        Debug.Log($"[HGM] Awake: cam={(_cameraTransform != null ? _cameraTransform.name : "NULL")}");
    }

    void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            _activated = true;
            _floorY = transform.position.y;
            Debug.Log($"[HGM] Start: sin NetworkManager → activado. pos:{transform.position} floorY:{_floorY:F3}");
        }
        else
        {
            Debug.Log($"[HGM] Start: esperando Activate()");
        }
    }

    public void Activate()
    {
        _activated = true;
        _floorY = transform.position.y;
        _verticalVelocity = 0f;
        _prevGrounded = true;
        Debug.Log($"[HGM] Activate | pos:{transform.position} floorY:{_floorY:F3}");
    }

    void Update()
    {
        if (!_activated)
        {
            if (Time.frameCount % 300 == 0)
                Debug.Log($"[HGM] no activado (frame {Time.frameCount})");
            return;
        }

        if (_handSubsystem == null || !_handSubsystem.running)
            _handSubsystem = GetHandSubsystem();

        // ── INPUT ─────────────────────────────────────────────────────────────
        bool keySpace  = Input.GetKey(KeyCode.Space);
        bool keyShiftL = Input.GetKey(KeyCode.LeftShift);
        bool keyShiftR = Input.GetKey(KeyCode.RightShift);
        bool keyW      = Input.GetKey(KeyCode.W);
        bool keyS      = Input.GetKey(KeyCode.S);
        bool keyA      = Input.GetKey(KeyCode.A);
        bool keyD      = Input.GetKey(KeyCode.D);
        bool anyKey    = keySpace || keyShiftL || keyShiftR || keyW || keyS || keyA || keyD;

        bool fistGesture = false;
        if (_handSubsystem != null && _handSubsystem.leftHand.isTracked)
            fistGesture = IsFist(_handSubsystem.leftHand);

        bool wantsToMove = keySpace || keyShiftL || keyShiftR || keyW || fistGesture;
        bool wantsBack   = keyS;
        bool wantsLeft   = keyA;
        bool wantsRight  = keyD;

        if (wantsToMove != _prevWantsMove)
        {
            Debug.Log($"[HGM] wantsToMove → {wantsToMove} f:{_debugFrames} pos:{transform.position:F2}");
            _prevWantsMove = wantsToMove;
        }

        // ── CLAMP: restore if OpenXR native tracking wrote a bad Y ───────────
        Vector3 pos = transform.position;
        if (!float.IsNaN(_floorY) && pos.y < _floorY - 0.5f)
        {
            Debug.LogWarning($"[HGM] U Y clamp f{_debugFrames}: {pos.y:F2} → {_floorY:F2}");
            pos.y = _floorY;
        }

        _debugFrames++;
        bool grounded = pos.y <= _floorY + 0.15f;
        if (grounded != _prevGrounded)
        {
            Debug.Log($"[HGM] grounded → {grounded} f{_debugFrames} pos:{pos:F2}");
            _prevGrounded = grounded;
        }

        // ── GRAVITY (no cc.Move — bypasses OpenXR CC physics interception) ────
        if (grounded)
        {
            _verticalVelocity = 0f;
            pos.y = _floorY;
        }
        else
        {
            _verticalVelocity += Gravity * Time.deltaTime;
            pos.y += _verticalVelocity * Time.deltaTime;
            if (pos.y < _floorY)
            {
                pos.y = _floorY;
                _verticalVelocity = 0f;
            }
        }

        // ── HORIZONTAL MOVEMENT ───────────────────────────────────────────────
        Vector3 prevPos = pos;
        if (_cameraTransform != null)
        {
            Vector3 fwd = _cameraTransform.forward; fwd.y = 0; fwd.Normalize();
            Vector3 rgt = _cameraTransform.right;   rgt.y = 0; rgt.Normalize();
            Vector3 delta = Vector3.zero;

            if (wantsToMove) delta += fwd;
            if (wantsBack)   delta -= fwd;
            if (wantsLeft)   delta -= rgt;
            if (wantsRight)  delta += rgt;

            if (delta.sqrMagnitude > 1f) delta.Normalize();
            delta *= speed * Time.deltaTime;
            pos.x += delta.x;
            pos.z += delta.z;
        }

        transform.position = pos;
    }

    void LateUpdate()
    {
        if (!_activated) return;

        // Guard against OpenXR writes that happen in LateUpdate before ours.
        Vector3 cur = transform.position;
        if (!float.IsNaN(_floorY) && cur.y < _floorY - 0.5f)
        {
            Debug.LogWarning($"[HGM] LU Y clamp f{_debugFrames}: {cur.y:F2} → {_floorY:F2}");
            cur.y = _floorY;
            transform.position = cur;
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
        var wrist     = hand.GetJoint(XRHandJointID.Wrist);
        if (middleTip.TryGetPose(out Pose tipPose) && wrist.TryGetPose(out Pose wristPose))
            return Vector3.Distance(tipPose.position, wristPose.position) < 0.08f;
        return false;
    }
}
