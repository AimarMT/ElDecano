using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : NetworkBehaviour
{
    [Header("Prefabs de Personaje")]
    [SerializeField] private GameObject[] playerPrefabs; // Aquí arrastras tus 2 prefabs

    [Header("UI Canvas")]
    [SerializeField] private GameObject selectionCanvas; // El canvas con los botones

    // Este método lo llamará el botón del Prefab 0
    public void SelectCharacter0() => RequestSpawnServerRpc(0, NetworkManager.Singleton.LocalClientId);

    // Este método lo llamará el botón del Prefab 1
    public void SelectCharacter1() => RequestSpawnServerRpc(1, NetworkManager.Singleton.LocalClientId);

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(int prefabIndex, ulong clientId)
    {
        // 1. Instanciamos el prefab elegido en el servidor
        GameObject playerObj = Instantiate(playerPrefabs[prefabIndex]);

        // 2. Lo spawneamos en la red dándole la autoría al cliente que lo pidió
        NetworkObject nObj = playerObj.GetComponent<NetworkObject>();
        nObj.SpawnAsPlayerObject(clientId);

        // 3. Ocultamos el canvas para ese cliente (vía RPC)
        HideCanvasClientRpc(rpcParams: new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
    }

    [ClientRpc]
    private void HideCanvasClientRpc(ClientRpcParams rpcParams = default)
    {
        if (selectionCanvas != null) selectionCanvas.SetActive(false);
    }
}