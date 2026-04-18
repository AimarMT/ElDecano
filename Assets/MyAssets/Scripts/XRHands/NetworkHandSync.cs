using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class NetworkHandSync : NetworkBehaviour
{
    [Header("Referencias Lado Izquierdo")]
    public GameObject leftHand;
    public GameObject leftController;

    [Header("Referencias Lado Derecho")]
    public GameObject rightHand;
    public GameObject rightController;

    public override void OnNetworkSpawn()
    {
        // Configuración de cámara y controles según quién sea el dueño
        if (IsOwner)
        {
            GetComponentInChildren<Camera>().enabled = true;
            var al = GetComponentInChildren<AudioListener>();
            if (al != null) al.enabled = true;
        }
        else
        {
            GetComponentInChildren<Camera>().enabled = false;
            var al = GetComponentInChildren<AudioListener>();
            if (al != null) al.enabled = false;

            var poseDriver = GetComponentInChildren<TrackedPoseDriver>();
            if (poseDriver != null) poseDriver.enabled = false;
        }
    }

    void Update()
    {
        // Si soy el dueño del avatar, le digo al servidor qué tengo activo
        if (IsOwner)
        {
            SyncVisualsServerRpc(
                leftHand.activeSelf,
                leftController.activeSelf,
                rightHand.activeSelf,
                rightController.activeSelf
            );
        }
    }

    [ServerRpc]
    private void SyncVisualsServerRpc(bool lh, bool lc, bool rh, bool rc)
    {
        SyncVisualsClientRpc(lh, lc, rh, rc);
    }

    [ClientRpc]
    private void SyncVisualsClientRpc(bool lh, bool lc, bool rh, bool rc)
    {
        // Los otros jugadores ven lo mismo que yo
        if (!IsOwner)
        {
            leftHand.SetActive(lh);
            leftController.SetActive(lc);
            rightHand.SetActive(rh);
            rightController.SetActive(rc);
        }
    }
}