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

    private int _noListoFrames  = 0;
    private int _nonOwnerFrame  = 0;
    private int _monitorFrame   = 0;
    private bool _monitorActive = false;
    private Vector3 _monitoredRootPos;

    // NetworkVariables — el dueño escribe, todos los demás leen automáticamente.
    // Esto funciona en modo host en ambas direcciones (host→cliente y cliente→host).
    private NetworkVariable<Vector3> _netPos  = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> _netRotY = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> _netLH = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> _netRH = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // ─────────────────────────────────────────────────────────────────────────
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[NP2] SPAWN | obj={name} netId={NetworkObjectId} " +
                  $"ownerId={OwnerClientId} localId={NetworkManager.Singleton.LocalClientId} " +
                  $"isOwner={IsOwner} isServer={IsServer} isHost={IsHost} isSpawned={IsSpawned}");

        if (IsOwner)
        {
            // ── Buscar el XR Rig ────────────────────────────────────────────
            XRRigReferences2[] rigs = FindObjectsByType<XRRigReferences2>(FindObjectsSortMode.None);
            foreach (var rig in rigs)
            {
                Camera cam = rig.head?.GetComponent<Camera>();
                if (cam != null && cam.isActiveAndEnabled) { myXR = rig; break; }
            }
            if (myXR == null)
            {
                Debug.LogWarning("[NP2] No se encontró rig con cámara activa, usando el primero.");
                if (rigs.Length > 0) myXR = rigs[0];
            }
            Debug.Log($"[NP2] OWNER: rig='{(myXR != null ? myXR.name : "NULL")}'");

            // ── Ocultar propio avatar (primera persona) ─────────────────────
            foreach (Renderer r in meshesToDisable)
                if (r != null) r.enabled = false;
            Debug.Log($"[NP2] OWNER: {meshesToDisable.Length} renderers desactivados (primera persona).");

            if (SceneManager.GetActiveScene().name == mapSceneName)
                StartCoroutine(MoverXRAlSpawn());
            else
                listo = true;
        }
        else
        {
            // ── Activar renderers para que los demás puedan ver este avatar ──
            foreach (Renderer r in meshesToDisable)
                if (r != null && !r.enabled) r.enabled = true;
            listo = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (IsOwner)
        {
            // ── OWNER: escribir posición en NetworkVariables ──────────────
            if (!listo)
                return;
            if (myXR == null || myXR.head == null) return;

            // Monitor
            if (_monitorActive && myXR.root != null)
            {
                Vector3 cur = myXR.root.position;
                float yDiff  = Mathf.Abs(cur.y - _monitoredRootPos.y);
                float xzDiff = new Vector2(cur.x - _monitoredRootPos.x, cur.z - _monitoredRootPos.z).magnitude;
                _monitorFrame++;
                if (yDiff > 0.5f || xzDiff > 0.5f)
                    Debug.Log($"[NP2] Monitor: root={cur:F3} Δy={yDiff:F2} ΔXZ={xzDiff:F2}");
                if (yDiff > 5f)  Debug.LogWarning($"[NP2] MONITOR: Y saltó {_monitoredRootPos.y:F2}→{cur.y:F2}");
                if (xzDiff > 5f) Debug.LogWarning($"[NP2] MONITOR: XZ saltó a {cur.x:F2},{cur.z:F2}");
                _monitoredRootPos = cur;
            }

            // Posicionar avatar en el head del XR rig
            transform.position = myXR.head.position;
            Vector3 fwd = myXR.head.forward; fwd.y = 0;
            if (fwd.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(fwd);
            if (head != null && head != transform)
                head.SetPositionAndRotation(myXR.head.position, myXR.head.rotation);
            if (leftHand  != null && myXR.leftHand  != null)
                leftHand.SetPositionAndRotation(myXR.leftHand.position, myXR.leftHand.rotation);
            if (rightHand != null && myXR.rightHand != null)
                rightHand.SetPositionAndRotation(myXR.rightHand.position, myXR.rightHand.rotation);

            // Escribir en NetworkVariables — NGO lo sincroniza automáticamente a todos
            _netPos.Value  = transform.position;
            _netRotY.Value = transform.eulerAngles.y;
            _netLH.Value   = myXR.leftHand  != null ? myXR.leftHand.position  : Vector3.zero;
            _netRH.Value   = myXR.rightHand != null ? myXR.rightHand.position : Vector3.zero;
        }
        else
        {
            // ── NO-OWNER: leer NetworkVariables y aplicar al transform ────
            _nonOwnerFrame++;

            transform.position = _netPos.Value;
            transform.rotation = Quaternion.Euler(0, _netRotY.Value, 0);
            if (head != null && head != transform)
                head.SetPositionAndRotation(_netPos.Value, Quaternion.Euler(0, _netRotY.Value, 0));
            if (leftHand  != null) leftHand.position  = _netLH.Value;
            if (rightHand != null) rightHand.position = _netRH.Value;

        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator MoverXRAlSpawn()
    {
        yield return null;
        yield return null;

        if (myXR == null || myXR.root == null)
        {
            Debug.LogWarning("[NP2] myXR o root es null, abortando spawn.");
            listo = true;
            yield break;
        }

        Debug.Log($"[NP2] MoverXRAlSpawn: spawnPos={transform.position} rigRoot={myXR.root.position}");

        var cc = myXR.root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Desactivar locomotion temporalmente
        var disabledLoco = new System.Collections.Generic.List<MonoBehaviour>();
        var searchRoots  = new System.Collections.Generic.List<Transform>();
        searchRoots.Add(myXR.root);
        if (myXR.root.parent != null)
            foreach (Transform s in myXR.root.parent)
                if (s != myXR.root) searchRoots.Add(s);

        foreach (var sr in searchRoots)
            foreach (var mb in sr.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string t = mb.GetType().Name;
                if ((t.Contains("MoveProvider") || t.Contains("Teleport") ||
                     t.Contains("BodyTransformer") || t.Contains("LocomotionMediator") ||
                     t.Contains("SnapTurn") || t.Contains("ContinuousMove")) &&
                    mb.enabled && !disabledLoco.Contains(mb))
                {
                    mb.enabled = false;
                    disabledLoco.Add(mb);
                }
            }

        // Detección de suelo
        Vector3 spawnPos = transform.position;
        float ccBottomOffset = cc != null ? (cc.center.y - cc.height / 2f) : -0.9f;
        Vector3 rayStart = new Vector3(spawnPos.x, spawnPos.y + 10f, spawnPos.z);
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 300f,
                                               Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => b.point.y.CompareTo(a.point.y));

        bool floorFound = false;
        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(myXR.root)) continue;
            if (h.point.y <= spawnPos.y + 0.3f)
            {
                spawnPos.y = h.point.y - ccBottomOffset + 0.05f;
                Debug.Log($"[NP2] Suelo '{h.collider.name}' Y={h.point.y:F3} → root Y={spawnPos.y:F3}");
                floorFound = true;
                break;
            }
        }
        if (!floorFound)
            Debug.LogWarning($"[NP2] Sin suelo bajo spawn. Y={spawnPos.y:F2}");

        myXR.root.position = spawnPos;
        myXR.root.rotation = transform.rotation;
        Physics.SyncTransforms();

        // Hold 8 frames
        float holdX = spawnPos.x, holdY = spawnPos.y, holdZ = spawnPos.z;
        for (int f = 0; f < 8; f++)
        {
            yield return null;
            Vector3 cur = myXR.root.position;
            bool drift = Mathf.Abs(cur.x - holdX) > 0.15f ||
                         Mathf.Abs(cur.z - holdZ) > 0.15f ||
                         Mathf.Abs(cur.y - holdY) > 1f;
            if (drift)
            {
                Debug.LogWarning($"[NP2] DERIVA f{f}: {cur:F2} → forzando a spawn");
                myXR.root.position = new Vector3(holdX, holdY, holdZ);
                Physics.SyncTransforms();
            }
            if (f == 0 && cc != null)
            {
                cc.enabled = true;
                Physics.SyncTransforms();
                cc.Move(Vector3.down * 0.15f);
            }
            else if (f > 0 && cc != null && !cc.isGrounded)
                cc.Move(Vector3.down * 0.15f);
        }

        // Grounding extra
        float elapsed = 0f;
        if (cc != null && !cc.isGrounded)
        {
            while (!cc.isGrounded && elapsed < 1f)
            {
                cc.Move(Vector3.down * 0.02f);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Reactivar locomotion excepto XRBodyTransformer/LocomotionMediator
        foreach (var mb in disabledLoco)
        {
            if (mb == null) continue;
            string t = mb.GetType().Name;
            if (t.Contains("BodyTransformer") || t.Contains("LocomotionMediator")) continue;
            mb.enabled = true;
        }

        // Settle 10 frames
        for (int f = 0; f < 10; f++)
        {
            yield return null;
            Vector3 cur = myXR.root.position;
            bool drift = Mathf.Abs(cur.x - holdX) > 0.15f ||
                         Mathf.Abs(cur.z - holdZ) > 0.15f ||
                         Mathf.Abs(cur.y - holdY) > 1f;
            if (drift) { myXR.root.position = new Vector3(holdX, holdY, holdZ); Physics.SyncTransforms(); }
            if (cc != null && !cc.isGrounded) cc.Move(Vector3.down * 0.3f);
        }

        Debug.Log($"[NP2] XR movido: root={myXR.root.position} grounded={cc?.isGrounded}");

        {
            var subs = new System.Collections.Generic.List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subs);
            foreach (var s in subs)
                if (s.running) s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
        }

        if (myXR.root != null)
        {
            _monitoredRootPos = myXR.root.position;
            _monitorActive = true;
        }

        var hgm = myXR.root.GetComponent<HandGestureMovement>();
        if (hgm != null) hgm.Activate();
        else Debug.LogWarning("[NP2] HandGestureMovement no encontrado.");

        listo = true;
        Debug.Log($"[NP2] listo=true. head={(head != null ? head.name : "NULL")} headEsRoot={head == transform} meshesToDisable:{meshesToDisable.Length}");
    }
}
