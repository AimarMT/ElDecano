using UnityEngine;
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
       Debug.Log("¡He intentado agarrar el objeto!"); // Esto aparecerá en la consola si el contacto físico funciona
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