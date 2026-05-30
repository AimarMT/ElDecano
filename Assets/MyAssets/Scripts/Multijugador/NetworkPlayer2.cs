using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkPlayer2 : NetworkBehaviour
{
    [Header("Referencias del Avatar (Hijos del Prefab)")]
    [SerializeField] Transform head;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [Header("Visuales")]
    [SerializeField] Renderer[] meshesToDisable;

    [Header("Escena del mapa")]
    public string mapSceneName = "MapaPillaPilla";

    private bool listo = false;
    private XRRigReferences2 myXR;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            XRRigReferences2[] rigs = FindObjectsByType<XRRigReferences2>(FindObjectsSortMode.None);

            foreach (var rig in rigs)
            {
                Camera cam = rig.head?.GetComponent<Camera>();
                if (cam != null && cam.isActiveAndEnabled)
                {
                    myXR = rig;
                    break;
                }
            }

            if (myXR == null)
            {
                Debug.LogWarning("[NP2] No se encontró rig con cámara activa, usando el primero.");
                if (rigs.Length > 0) myXR = rigs[0];
            }

            foreach (Renderer r in meshesToDisable)
                if (r != null) r.enabled = false;

            if (SceneManager.GetActiveScene().name == mapSceneName)
                StartCoroutine(MoverXRAlSpawn());
            else
                listo = true;
        }
        else
        {
            // The prefab may have these renderers disabled by default (so the local
            // owner doesn't see their own body in first-person VR).  Ensure they are
            // visible to every OTHER player who receives this instance.
            int enabled = 0;
            foreach (Renderer r in meshesToDisable)
            {
                if (r != null && !r.enabled)
                {
                    r.enabled = true;
                    enabled++;
                }
            }
            Debug.Log($"[NP2] Non-owner spawn: {enabled}/{meshesToDisable.Length} renderers habilitados para visibilidad.");
            listo = true;
        }
    }

    private IEnumerator MoverXRAlSpawn()
    {
        // Wait two frames so NGO has finished applying the initial network position.
        yield return null;
        yield return null;

        if (myXR == null || myXR.root == null)
        {
            Debug.LogWarning("[NP2] myXR o root es null, abortando spawn.");
            listo = true;
            yield break;
        }

        Debug.Log($"[NP2] MoverXRAlSpawn iniciado. transform.position={transform.position}");

        // Force Device tracking mode so the OpenXR runtime stops overriding root.Y
        // with the physical floor offset every time cc.Move is called.
        {
            var inputSubs = new System.Collections.Generic.List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(inputSubs);
            int changed = 0;
            foreach (var s in inputSubs)
                if (s.running && s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device))
                    changed++;
            Debug.Log($"[NP2] TrackingOriginMode → Device: {changed}/{inputSubs.Count} subsistemas.");
        }

        var cc = myXR.root.GetComponent<CharacterController>();
        Debug.Log($"[NP2] rig='{myXR.name}' root='{myXR.root?.name}' " +
                  $"cc={(cc != null ? "OK" : "NULL")} " +
                  $"cc.height={cc?.height:F2} cc.radius={cc?.radius:F2} cc.center={cc?.center} " +
                  $"root.pos={myXR.root.position} head.pos={myXR.head?.position}");

        if (cc != null)
        {
            Debug.Log($"[NP2] Desactivando CC. cc.enabled era:{cc.enabled} pos:{myXR.root.position:F3}");
            cc.enabled = false;
        }

        // Disable locomotion components so they can't hijack the root during teleport.
        // Search: inside XR rig hierarchy + siblings in parent container.
        System.Collections.Generic.List<MonoBehaviour> disabledLoco =
            new System.Collections.Generic.List<MonoBehaviour>();

        // Collect search roots: the XR rig itself, and all siblings (children of its parent).
        System.Collections.Generic.List<Transform> searchRoots =
            new System.Collections.Generic.List<Transform>();
        searchRoots.Add(myXR.root);
        if (myXR.root.parent != null)
            foreach (Transform sibling in myXR.root.parent)
                if (sibling != myXR.root) searchRoots.Add(sibling);

        foreach (var searchRoot in searchRoots)
        {
            foreach (var mb in searchRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string typeName = mb.GetType().Name;
                if (typeName.Contains("MoveProvider") || typeName.Contains("Teleport") ||
                    typeName.Contains("BodyTransformer") || typeName.Contains("LocomotionMediator") ||
                    typeName.Contains("SnapTurn") || typeName.Contains("ContinuousMove"))
                {
                    if (mb.enabled && !disabledLoco.Contains(mb))
                    {
                        mb.enabled = false;
                        disabledLoco.Add(mb);
                        Debug.Log($"[NP2] Desactivado '{typeName}' en '{mb.gameObject.name}'.");
                    }
                }
            }
        }

        Vector3 spawnPos = transform.position;

        // CC bottom offset relative to the root origin (negative => bottom is below root).
        // Accounts for cc.center being offset upward (e.g. center.y≈0.76, height 1.80).
        float ccBottomOffset = cc != null ? (cc.center.y - cc.height / 2f) : -0.9f;

        // ── ROBUST FLOOR DETECTION ────────────────────────────────────────────
        // Cast from 10 m above the spawn, down 300 m, ignoring triggers and the
        // player's own colliders. Pick the highest surface at/below the feet.
        Vector3 rayStart = new Vector3(spawnPos.x, spawnPos.y + 10f, spawnPos.z);
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 300f,
                                               Physics.DefaultRaycastLayers,
                                               QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => b.point.y.CompareTo(a.point.y)); // highest first

        bool floorFound = false;
        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(myXR.root)) continue; // skip own body
            if (h.point.y <= spawnPos.y + 0.3f)                       // at/below feet
            {
                // Place root so the CC bottom sits 5 cm above this floor.
                spawnPos.y = h.point.y - ccBottomOffset + 0.05f;
                Debug.Log($"[NP2] Suelo '{h.collider.name}' Y={h.point.y:F3} → root Y={spawnPos.y:F3} " +
                          $"(bottomOff={ccBottomOffset:F2})");
                floorFound = true;
                break;
            }
        }
        if (!floorFound)
            Debug.LogWarning($"[NP2] NINGÚN suelo bajo X={spawnPos.x:F2} Z={spawnPos.z:F2} en 300m " +
                             $"({hits.Length} hits). Y se mantiene en {spawnPos.y:F2}");

        myXR.root.position = spawnPos;
        myXR.root.rotation = transform.rotation;
        Physics.SyncTransforms();

        Debug.Log($"[NP2] root placed → {myXR.root.position}. Iniciando hold XZ por 8 frames...");

        // ── PER-FRAME HOLD (8 frames) ─────────────────────────────────────────
        // Each frame: detect XYZ drift caused by external systems and correct it.
        // CC is re-enabled on frame 0 so physics grounding can settle.
        float holdX = spawnPos.x;
        float holdY = spawnPos.y;
        float holdZ = spawnPos.z;

        for (int f = 0; f < 8; f++)
        {
            yield return null;

            Vector3 cur = myXR.root.position;
            bool drifted = Mathf.Abs(cur.x - holdX) > 0.15f ||
                           Mathf.Abs(cur.z - holdZ) > 0.15f ||
                           Mathf.Abs(cur.y - holdY) > 1f;   // floor-tracking mode can drop Y 50m

            if (drifted)
            {
                Debug.LogWarning($"[NP2] DERIVA f{f}: root fue a {cur:F2}, " +
                                 $"forzando X={holdX:F2} Y={holdY:F2} Z={holdZ:F2}");
                cur.x = holdX;
                cur.y = holdY;
                cur.z = holdZ;
                myXR.root.position = cur;
                Physics.SyncTransforms();
            }

            Debug.Log($"[NP2] hold f{f}: root={myXR.root.position} " +
                      $"head={myXR.head?.position} grounded={cc?.isGrounded} drift={drifted}");

            if (f == 0)
            {
                if (cc != null)
                {
                    Debug.Log($"[NP2] Reactivando CC en hold f0. pos antes:{myXR.root.position:F3}");
                    cc.enabled = true;
                    Physics.SyncTransforms();
                    Debug.Log($"[NP2] CC reactivado. pos tras sync:{myXR.root.position:F3} grounded:{cc.isGrounded}");
                    cc.Move(Vector3.down * 0.15f);
                    Debug.Log($"[NP2] cc.Move(down 0.15) → pos:{myXR.root.position:F3} grounded:{cc.isGrounded}");
                }
            }
            else if (cc != null && !cc.isGrounded)
            {
                Debug.Log($"[NP2] hold f{f} no grounded → cc.Move(down). pos:{myXR.root.position:F3}");
                cc.Move(Vector3.down * 0.15f);
            }
        }

        Debug.Log($"[NP2] Tras hold: root={myXR.root.position} grounded={cc?.isGrounded}");

        // Extra grounding wait (up to 1 s) if not yet grounded.
        float elapsed = 0f;
        if (cc != null && !cc.isGrounded)
        {
            float timeout = 1f;
            while (!cc.isGrounded && elapsed < timeout)
            {
                cc.Move(Vector3.down * 0.02f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Debug.Log($"[NP2] Grounding loop: grounded={cc.isGrounded} espera={elapsed:F2}s");
        }

        // Re-enable locomotion components EXCEPT XRBodyTransformer.
        // XRBodyTransformer intercepts every cc.Move call and teleports the root
        // ~32 m in XZ (the physical room offset), making horizontal movement impossible.
        int reactivated = 0;
        foreach (var mb in disabledLoco)
        {
            if (mb == null) continue;
            string typeName = mb.GetType().Name;
            if (typeName.Contains("BodyTransformer") || typeName.Contains("LocomotionMediator"))
            {
                Debug.Log($"[NP2] Manteniendo desactivado '{typeName}'.");
                continue;
            }
            mb.enabled = true;
            reactivated++;
        }
        Debug.Log($"[NP2] {reactivated} componentes reactivados. XRBodyTransformer y LocomotionMediator permanecen OFF.");

        // Extra settle phase: let locomotion initialize while keeping CC grounded.
        // The XRBodyTransformer can drift or lift the CC during its first frames.
        for (int f = 0; f < 10; f++)
        {
            yield return null;

            Vector3 cur = myXR.root.position;
            bool driftedXYZ = Mathf.Abs(cur.x - holdX) > 0.15f ||
                               Mathf.Abs(cur.z - holdZ) > 0.15f ||
                               Mathf.Abs(cur.y - holdY) > 1f;
            if (driftedXYZ)
            {
                cur.x = holdX;
                cur.y = holdY;
                cur.z = holdZ;
                myXR.root.position = cur;
                Physics.SyncTransforms();
                Debug.Log($"[NP2] settle drift f{f}: corregido a {cur:F2}");
            }
            if (cc != null && !cc.isGrounded)
            {
                cc.Move(Vector3.down * 0.3f);
                Debug.Log($"[NP2] settle grounding f{f}: cc.Move down");
            }
        }

        Debug.Log($"[NP2] XR movido: root={myXR.root.position} | grounded:{cc?.isGrounded} espera:{elapsed:F2}s");

        // Re-apply Device tracking mode: XRBodyTransformer may have reverted to Floor
        // when it was re-enabled during the settle phase.
        {
            var inputSubs = new System.Collections.Generic.List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(inputSubs);
            int changed = 0;
            foreach (var s in inputSubs)
                if (s.running && s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device))
                    changed++;
            Debug.Log($"[NP2] TrackingOriginMode → Device (2nd): {changed}/{inputSubs.Count}");
        }

        // Start position monitor BEFORE activating movement, so we can see
        // any external writes that happen during/after HGM activation.
        if (myXR.root != null)
        {
            _monitoredRootPos = myXR.root.position;
            _monitorActive = true;
            Debug.Log($"[NP2] Monitor de posición ACTIVADO. root:{myXR.root.position:F3}");
        }

        var movement = myXR.root.GetComponent<HandGestureMovement>();
        if (movement != null) movement.Activate();
        else Debug.LogWarning("[NP2] HandGestureMovement no encontrado en root!");

        listo = true;
        Debug.Log($"[NP2] listo=true (owner). head={(head != null ? head.name : "NULL")} headEsRoot={head == transform} meshesToDisable:{meshesToDisable.Length}");
    }

    // Position monitor: detects external writes to the XR root between frames.
    private Vector3 _monitoredRootPos;
    private bool _monitorActive = false;
    private int _monitorFrame = 0;
    private int _rpcSentCount = 0;
    private int _clientRpcCount = 0;
    private int _noListoFrames = 0;

    void Update()
    {
        if (!listo)
        {
            _noListoFrames++;
            if (_noListoFrames <= 3 || _noListoFrames % 120 == 0)
                Debug.Log($"[NP2] Update: no listo (frame {_noListoFrames}) isOwner:{IsOwner}");
            return;
        }
        if (!IsOwner || myXR == null) return;
        if (myXR.head == null) return;

        // ── POSITION MONITOR ────────────────────────────────────────────────
        if (_monitorActive && myXR.root != null)
        {
            Vector3 currentRoot = myXR.root.position;
            float yDiff  = Mathf.Abs(currentRoot.y - _monitoredRootPos.y);
            float xzDiff = new Vector2(currentRoot.x - _monitoredRootPos.x, currentRoot.z - _monitoredRootPos.z).magnitude;
            _monitorFrame++;

            // Log every 30 frames or when significant change
            if (_monitorFrame % 30 == 0 || yDiff > 0.5f || xzDiff > 0.5f)
                Debug.Log($"[NP2] Monitor f{_monitorFrame}: root={currentRoot:F3} prev={_monitoredRootPos:F3} Δy={yDiff:F2} ΔXZ={xzDiff:F2} cc={(myXR.root.GetComponent<CharacterController>()?.enabled == true ? "ON" : "OFF")} grounded={myXR.root.GetComponent<CharacterController>()?.isGrounded}");

            if (yDiff > 5f)
                Debug.LogWarning($"[NP2] MONITOR: root Y saltó {_monitoredRootPos.y:F2}→{currentRoot.y:F2} ({yDiff:F1}m) entre frames");
            if (xzDiff > 5f)
                Debug.LogWarning($"[NP2] MONITOR: root XZ saltó ({_monitoredRootPos.x:F2},{_monitoredRootPos.z:F2})→({currentRoot.x:F2},{currentRoot.z:F2}) ({xzDiff:F1}m) entre frames");

            _monitoredRootPos = currentRoot;
        }

        // Avatar root = head mesh (AndoniCabeza/ProtaCabeza have head==root transform).
        // Position at XR camera height (eye level), not at rig floor level.
        transform.position = myXR.head.position;
        Vector3 camForward = myXR.head.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(camForward);

        if (head != null && head != transform)
            head.SetPositionAndRotation(myXR.head.position, myXR.head.rotation);
        if (leftHand != null && myXR.leftHand != null)
            leftHand.SetPositionAndRotation(myXR.leftHand.position, myXR.leftHand.rotation);
        if (rightHand != null && myXR.rightHand != null)
            rightHand.SetPositionAndRotation(myXR.rightHand.position, myXR.rightHand.rotation);

        _rpcSentCount++;
        if (_rpcSentCount <= 5 || _rpcSentCount % 300 == 0)
            Debug.Log($"[NP2] ServerRpc #{_rpcSentCount} enviando: pos={transform.position:F2} head={myXR.head.position:F2} headEsRoot={head == transform}");

        UpdatePositionServerRpc(
            transform.position, transform.rotation,
            myXR.head.position, myXR.head.rotation,
            myXR.leftHand != null ? myXR.leftHand.position : Vector3.zero,
            myXR.leftHand != null ? myXR.leftHand.rotation : Quaternion.identity,
            myXR.rightHand != null ? myXR.rightHand.position : Vector3.zero,
            myXR.rightHand != null ? myXR.rightHand.rotation : Quaternion.identity
        );
    }

    [ServerRpc]
    void UpdatePositionServerRpc(
        Vector3 pos, Quaternion rot,
        Vector3 hPos, Quaternion hRot,
        Vector3 lhPos, Quaternion lhRot,
        Vector3 rhPos, Quaternion rhRot)
    {
        transform.SetPositionAndRotation(pos, rot);
        if (head != null && head != transform) head.SetPositionAndRotation(hPos, hRot);
        if (leftHand != null) leftHand.SetPositionAndRotation(lhPos, lhRot);
        if (rightHand != null) rightHand.SetPositionAndRotation(rhPos, rhRot);
        UpdatePositionClientRpc(pos, rot, hPos, hRot, lhPos, lhRot, rhPos, rhRot);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(
        Vector3 pos, Quaternion rot,
        Vector3 hPos, Quaternion hRot,
        Vector3 lhPos, Quaternion lhRot,
        Vector3 rhPos, Quaternion rhRot)
    {
        if (IsOwner) return;
        _clientRpcCount++;
        if (_clientRpcCount <= 5 || _clientRpcCount % 300 == 0)
            Debug.Log($"[NP2] ClientRpc #{_clientRpcCount}: pos={pos:F2} hPos={hPos:F2} headEsRoot={head == transform}");
        transform.SetPositionAndRotation(pos, rot);
        if (head != null && head != transform) head.SetPositionAndRotation(hPos, hRot);
        if (leftHand != null) leftHand.SetPositionAndRotation(lhPos, lhRot);
        if (rightHand != null) rightHand.SetPositionAndRotation(rhPos, rhRot);
    }
}
