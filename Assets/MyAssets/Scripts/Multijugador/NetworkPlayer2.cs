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
    private XRRigReferences2 myXR; // Referencia LOCAL a nuestro propio rig

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[NetworkPlayer2] OnNetworkSpawn — IsOwner:{IsOwner} OwnerClientId:{OwnerClientId} pos:{transform.position} NetworkObjectId:{NetworkObjectId}");

        if (IsOwner)
        {
            // Buscamos TODOS los XRRigReferences2 de la escena y cogemos
            // el que pertenece a este proceso (el que tiene la cámara activa)
            XRRigReferences2[] rigs = FindObjectsByType<XRRigReferences2>(FindObjectsSortMode.None);
            Debug.Log($"[NetworkPlayer2] Rigs encontrados en escena: {rigs.Length}");

            foreach (var rig in rigs)
            {
                // El rig local es el que tiene la Main Camera activa
                Camera cam = rig.head?.GetComponent<Camera>();
                if (cam != null && cam.isActiveAndEnabled)
                {
                    myXR = rig;
                    Debug.Log($"[NetworkPlayer2] Rig propio encontrado: {rig.gameObject.name}");
                    break;
                }
            }

            if (myXR == null)
            {
                // Fallback: cogemos el primero que haya
                Debug.LogWarning("[NetworkPlayer2] No se encontró rig con cámara activa, usando el primero disponible.");
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
            listo = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsOwner)
            Debug.Log($"[NetworkPlayer2] OnNetworkDespawn — NetworkObjectId:{NetworkObjectId}");
    }

    private IEnumerator MoverXRAlSpawn()
    {
        yield return null;

        if (myXR == null || myXR.root == null)
        {
            Debug.LogWarning("[NetworkPlayer2] myXR o root es null, no se puede mover al spawn.");
            listo = true;
            yield break;
        }

        var cc = myXR.root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        myXR.root.position = transform.position;
        myXR.root.rotation = transform.rotation;

        yield return null; // Frame extra para que el CC registre la nueva posición

        if (cc != null) cc.enabled = true;

        Debug.Log($"[NetworkPlayer2] XR Origin movido a {transform.position}");
        listo = true;
    }

    void Update()
    {
        if (!listo) return;
        if (!IsOwner || myXR == null) return;
        if (myXR.head == null) return;

        transform.position = myXR.head.position;
        Vector3 camForward = myXR.head.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(camForward);

        if (head != null)
            head.SetPositionAndRotation(myXR.head.position, myXR.head.rotation);
        if (leftHand != null && myXR.leftHand != null)
            leftHand.SetPositionAndRotation(myXR.leftHand.position, myXR.leftHand.rotation);
        if (rightHand != null && myXR.rightHand != null)
            rightHand.SetPositionAndRotation(myXR.rightHand.position, myXR.rightHand.rotation);

        UpdatePositionServerRpc(
            transform.position,
            transform.rotation,
            head != null ? head.position : Vector3.zero,
            head != null ? head.rotation : Quaternion.identity,
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
        if (head != null) head.SetPositionAndRotation(hPos, hRot);
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
        transform.SetPositionAndRotation(pos, rot);
        if (head != null) head.SetPositionAndRotation(hPos, hRot);
        if (leftHand != null) leftHand.SetPositionAndRotation(lhPos, lhRot);
        if (rightHand != null) rightHand.SetPositionAndRotation(rhPos, rhRot);
    }
}