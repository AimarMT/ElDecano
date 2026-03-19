using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkGrabHandler : NetworkBehaviour
{
    private XRGrabInteractable _grabInteractable;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    public override void OnNetworkSpawn()
    {
        // Suscribirse al evento de agarre
        _grabInteractable.selectEntered.AddListener(OnGrab);
    }

    public override void OnNetworkDespawn()
    {
        // Limpiar el listener al destruir o desactivar el objeto de red
        _grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Si no somos el dueño del objeto, le pedimos al servidor que nos dé la propiedad
        if (IsClient && !IsOwner)
        {
            Debug.Log($"[Red] Intentando tomar posesión de: {gameObject.name}");
            RequestOwnershipRpc();
        }
    }

    // Atributo moderno de Unity Netcode: Enviamos al Servidor, permiso para Todos (Everyone)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipRpc(RpcParams rpcParams = default)
    {
        // El servidor recibe la petición y cambia el dueño al cliente que envió el mensaje
        var networkObject = GetComponent<NetworkObject>();
        networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        
        Debug.Log($"[Servidor] Propiedad de {gameObject.name} otorgada al cliente {rpcParams.Receive.SenderClientId}");
    }
}