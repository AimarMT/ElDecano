using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(NetworkObject), typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class NetworkedGrab : NetworkBehaviour
{
    private NetworkObject networkObject;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        interactable.selectEntered.AddListener((args) => RequestOwnership());
        interactable.selectExited.AddListener((args) => ReleaseOwnership());
    }

    private void RequestOwnership()
    {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestOwnershipRpc(clientID);
    }

    private void ReleaseOwnership()
    {
        ReleaseOwnershipRpc();
    }


    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong clientID)
    {
        Debug.Log("changing ownership");
        networkObject.ChangeOwnership(clientID);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseOwnershipRpc()
    {
        Debug.Log("releasing ownership");
        networkObject.RemoveOwnership();
    }


}
