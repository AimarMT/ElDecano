using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ClientOnlyGrab : NetworkBehaviour
{
    private XRGrabInteractable grabInteractable;
    private NetworkTransform netTransform;
    private InteractionLayerMask originalLayers;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkObject.AutoObjectParentSync = false;
    }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        netTransform = GetComponent<NetworkTransform>();
        originalLayers = grabInteractable.interactionLayers;
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    public override void OnNetworkDespawn()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        base.OnNetworkDespawn();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Si es un socket, desactivar sincronización de red
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
        {
            if (netTransform != null)
                netTransform.enabled = false;

            // Sincronizar posición final al servidor
            SyncPositionRpc(transform.position, transform.rotation);
            return;
        }

        if (!IsClientOnly())
        {
            Debug.Log("El host no puede agarrar esta llave.");
            grabInteractable.interactionLayers = InteractionLayerMask.GetMask();
            StartCoroutine(ReenableNextFrame());
            return;
        }

        // Reactivar sincronización al agarrar con mano
        if (netTransform != null)
            netTransform.enabled = true;

        if (!IsOwner)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            RequestOwnershipRpc(clientId);
            grabInteractable.interactionLayers = InteractionLayerMask.GetMask();
            StartCoroutine(WaitForOwnership());
            return;
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (IsOwner)
        {
            SyncPositionRpc(transform.position, transform.rotation);
        }
    }

    private bool IsClientOnly()
    {
        return NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
    }

    private IEnumerator ReenableNextFrame()
    {
        yield return null;
        grabInteractable.interactionLayers = originalLayers;
    }

    private IEnumerator WaitForOwnership()
    {
        while (!IsOwner)
            yield return null;
        grabInteractable.interactionLayers = originalLayers;
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong requestingClientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(requestingClientId) &&
            requestingClientId != NetworkManager.ServerClientId)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.ChangeOwnership(requestingClientId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SyncPositionRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}