using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer2 : NetworkBehaviour
{
    [Header("Referencias del Avatar (Hijos del Prefab)")]
    [SerializeField] Transform head;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [Header("Visuales")]
    [SerializeField] Renderer[] meshesToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Ocultamos nuestras mallas para no ver el interior de nuestra cabeza
            foreach (Renderer r in meshesToDisable)
            {
                if (r != null) r.enabled = false;
            }
        }
    }

    void Update()
    {
        // Solo el dueño localiza su cámara y manda la posición a los demás
        if (!IsOwner || XRRigReferences2.Instance == null) return;

        var xr = XRRigReferences2.Instance;
        if (xr.head == null) return;

        // --- SOLUCIÓN DEFINITIVA: SEGUIMIENTO GLOBAL ---

        // 1. Movemos el objeto raíz del avatar a la posición de la cámara en el mundo
        // Al usar .position (global), el avatar te seguirá aunque el XR Origin esté quieto
        transform.position = xr.head.position;

        // 2. Rotamos el cuerpo para que mire hacia donde mira la cámara (solo eje Y)
        Vector3 camForward = xr.head.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(camForward);
        }

        // 3. Posicionamos las manos y la cabeza del prefab en sus sitios globales
        if (head != null) head.SetPositionAndRotation(xr.head.position, xr.head.rotation);

        if (leftHand != null && xr.leftHand != null)
            leftHand.SetPositionAndRotation(xr.leftHand.position, xr.leftHand.rotation);

        if (rightHand != null && xr.rightHand != null)
            rightHand.SetPositionAndRotation(xr.rightHand.position, xr.rightHand.rotation);

        // 4. Sincronizamos con el servidor
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