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

        // Auto-asignación para que el prefab no dé errores de referencia propia
        if (root == null) root = transform;
        if (head == null) head = transform;
        if (leftHand == null) leftHand = transform;
        if (rightHand == null) rightHand = transform;

        if (meshesToDisable != null)
        {
            foreach (Renderer r in meshesToDisable)
            {
                if (r != null) r.enabled = false;
            }
        }
    }

    void Update()
    {
        // 1. Verificamos que seamos el dueño y que exista el Singleton del Rig
        if (!IsOwner || XRRigReferences.Instance == null) return;

        var xr = XRRigReferences.Instance;

        // 2. VERIFICACIÓN DE SEGURIDAD: Solo movemos si las piezas del Rig están asignadas en la escena
        if (root != null && xr.root != null)
            root.SetPositionAndRotation(xr.root.position, xr.root.rotation);

        if (head != null && xr.head != null)
            head.SetPositionAndRotation(xr.head.position, xr.head.rotation);

        if (leftHand != null && xr.leftHand != null)
            leftHand.SetPositionAndRotation(xr.leftHand.position, xr.leftHand.rotation);

        if (rightHand != null && xr.rightHand != null)
            rightHand.SetPositionAndRotation(xr.rightHand.position, xr.rightHand.rotation);
    }
}