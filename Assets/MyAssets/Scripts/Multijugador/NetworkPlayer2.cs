using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer2 : NetworkBehaviour
{
    [SerializeField] Transform root;
    [SerializeField] Transform head;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [SerializeField] Renderer[] meshesToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        if (root == null) root = transform;
        if (head == null) head = transform;

        foreach (Renderer r in meshesToDisable)
        {
            if (r != null) r.enabled = false;
        }
    }

    void Update()
    {
        // Solo el dueño de este avatar (sea Host o Cliente) manda su posición
        if (!IsOwner || XRRigReferences.Instance == null) return;

        var xr = XRRigReferences.Instance;

        // 1. Moverse a sí mismo localmente
        if (xr.root != null) root.SetPositionAndRotation(xr.root.position, xr.root.rotation);
        if (xr.head != null) head.SetPositionAndRotation(xr.head.position, xr.head.rotation);
        if (xr.leftHand != null && leftHand != null) leftHand.SetPositionAndRotation(xr.leftHand.position, xr.leftHand.rotation);
        if (xr.rightHand != null && rightHand != null) rightHand.SetPositionAndRotation(xr.rightHand.position, xr.rightHand.rotation);

        // 2. Avisar al resto del mundo
        UpdatePositionServerRpc(root.position, root.rotation, head.position, head.rotation);
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 pos, Quaternion rot, Vector3 hPos, Quaternion hRot)
    {
        // El servidor actualiza su versión de este jugador
        root.SetPositionAndRotation(pos, rot);
        if (head != null) head.SetPositionAndRotation(hPos, hRot);

        // 3. ¡ESTA ES LA CLAVE! El servidor avisa a todos los demás clientes (ClientRpc)
        UpdatePositionClientRpc(pos, rot, hPos, hRot);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 pos, Quaternion rot, Vector3 hPos, Quaternion hRot)
    {
        // Si yo soy el que envió la posición, ignoro este mensaje (ya me moví en Update)
        if (IsOwner) return;

        // Los demás clientes mueven la esfera de este jugador en sus pantallas
        root.SetPositionAndRotation(pos, rot);
        if (head != null) head.SetPositionAndRotation(hPos, hRot);
    }
}