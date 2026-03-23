using Unity.Netcode;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;      // Asigna tu prefab aquí
    [SerializeField] private Transform[] spawnPoints;      // Opcional: puntos de aparición

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Solo el servidor crea jugadores

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Si es host, spawnear también para el propio host
        if (NetworkManager.Singleton.IsHost)
        {
            OnClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Elegir punto de spawn (si no tienes spawnPoints, usa Vector3.zero)
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            spawnPos = spawnPoints[idx].position;
            spawnRot = spawnPoints[idx].rotation;
        }

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}