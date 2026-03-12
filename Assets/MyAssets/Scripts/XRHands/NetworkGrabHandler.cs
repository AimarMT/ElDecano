using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkGrabHandler : NetworkBehaviour //Este script es para avisar al servidor que se ha cogido un objeto
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Escuchamos cuando alguien intenta agarrar el objeto
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Si soy un cliente y agarro el objeto, pido permiso al servidor para ser el dueño
        if (IsClient)
        {
            RequestOwnershipServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        // El servidor nos da el control del objeto
        GetComponent<NetworkObject>().ChangeOwnership(rpcParams.Receive.SenderClientId);
    }
}