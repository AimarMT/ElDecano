using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawner1; // Para el Host
    [SerializeField] private Transform spawner2; // Para el Client

    private bool spawnHecho = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectado;
    }

    private void OnClientConectado(ulong clientId)
    {
        if (!IsServer || spawnHecho) return;

        // Esperamos a que haya 2 jugadores (Host + Client)
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            spawnHecho = true;
            SpawnJugadores();
        }
    }

    private void SpawnJugadores()
    {
        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform punto = i == 0 ? spawner1 : spawner2;
            GameObject jugador = Instantiate(playerPrefab, punto.position, punto.rotation);
            jugador.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            Debug.Log($"[SpawnManager] Jugador {client.ClientId} spawneado en {punto.name}");
            i++;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConectado;
    }
}