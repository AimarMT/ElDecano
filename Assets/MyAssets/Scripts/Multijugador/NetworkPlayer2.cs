using Unity.Netcode;
using UnityEngine;
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            foreach (Renderer r in meshesToDisable)
                if (r != null) r.enabled = false;

            if (SceneManager.GetActiveScene().name == mapSceneName)
                StartCoroutine(MoverXRAlSpawn());
            else
                listo = true;
        }
        else
        {
            listo = true;
        }
    }

    private IEnumerator MoverXRAlSpawn()
    {
        yield return null;
        yield return null;

        if (XRRigReferences2.Instance != null && XRRigReferences2.Instance.root != null)
        {
            XRRigReferences2.Instance.root.position = transform.position;
            XRRigReferences2.Instance.root.rotation = transform.rotation;
            Debug.Log($"[NetworkPlayer2] XR Origin movido a {transform.position}");
        }

        listo = true;
    }

    void Update()
    {
        if (!listo) return;
        if (!IsOwner || XRRigReferences2.Instance == null) return;

        var xr = XRRigReferences2.Instance;
        if (xr.head == null) return;

        transform.position = xr.head.position;

        Vector3 camForward = xr.head.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(camForward);

        if (head != null) head.SetPositionAndRotation(xr.head.position, xr.head.rotation);

        if (leftHand != null && xr.leftHand != null)
            leftHand.SetPositionAndRotation(xr.leftHand.position, xr.leftHand.rotation);

        if (rightHand != null && xr.rightHand != null)
            rightHand.SetPositionAndRotation(xr.rightHand.position, xr.rightHand.rotation);

        UpdatePositionServerRpc(transform.position, transform.rotation, head.position, head.rotation);
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 pos, Quaternion rot, Vector3 hPos, Quaternion hRot)
    {
        transform.SetPositionAndRotation(pos, rot);
        if (head != null) head.SetPositionAndRotation(hPos, hRot);
        UpdatePositionClientRpc(pos, rot, hPos, hRot);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 pos, Quaternion rot, Vector3 hPos, Quaternion hRot)
    {
        if (IsOwner) return;
        transform.SetPositionAndRotation(pos, rot);
        if (head != null) head.SetPositionAndRotation(hPos, hRot);
    }
}