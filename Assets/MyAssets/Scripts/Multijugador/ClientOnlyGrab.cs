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
    private Rigidbody rb;
    private InteractionLayerMask originalLayers;
    private bool _wasKinematic;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkObject.AutoObjectParentSync = false;
    }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        netTransform = GetComponent<NetworkTransform>();
        rb = GetComponent<Rigidbody>();
        originalLayers = grabInteractable.interactionLayers;

        grabInteractable.throwOnDetach = false;

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
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
        {
            if (netTransform != null)
                netTransform.enabled = false;
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

        if (netTransform != null)
            netTransform.enabled = false;

        if (!IsOwner)
        {
            if (rb != null)
            {
                _wasKinematic = rb.isKinematic;
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            ulong clientId = NetworkManager.Singleton.LocalClientId;
            RequestOwnershipRpc(clientId);
            StartCoroutine(WaitForOwnershipThenActivate());
            return;
        }

        StabilizeRigidbody();
        if (netTransform != null)
            netTransform.enabled = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (IsOwner)
            SyncPositionRpc(transform.position, transform.rotation);

        StartCoroutine(StabilizeOnRelease());
    }

    private IEnumerator StabilizeOnRelease()
    {
        // Esperar 1 frame a que XRI termine su proceso de release
        yield return null;

        // Congelar el Rigidbody para eliminar CUALQUIER fuerza residual
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return null;

        // Restaurar física normal — solo caerá por gravedad
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        if (netTransform != null)
            netTransform.enabled = true;
    }

    private IEnumerator WaitForOwnershipThenActivate()
    {
        float timeout = 2f;
        while (!IsOwner && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!IsOwner)
        {
            Debug.LogWarning("[ClientOnlyGrab] Timeout esperando ownership.");
            if (rb != null)
                rb.isKinematic = _wasKinematic;
            yield break;
        }

        if (rb != null)
        {
            rb.isKinematic = _wasKinematic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (netTransform != null)
            netTransform.enabled = true;
    }

    private IEnumerator ReenableNextFrame()
    {
        yield return null;
        grabInteractable.interactionLayers = originalLayers;
    }

    private void StabilizeRigidbody()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private bool IsClientOnly()
    {
        return NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong requestingClientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(requestingClientId) &&
            requestingClientId != NetworkManager.ServerClientId)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.ChangeOwnership(requestingClientId);
        }
    }

    [Rpc(SendTo.Server)]
    private void SyncPositionRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}