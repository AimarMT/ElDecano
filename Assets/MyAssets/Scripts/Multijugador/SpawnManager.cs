using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefabHost;   // Perseguidor
    [SerializeField] private GameObject prefabClient; // Perseguido
    [SerializeField] private Transform spawner1;      // Para el Host
    [SerializeField] private Transform spawner2;      // Para el Client

    private bool spawnHecho = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectado;

        StartCoroutine(DestruirAutoSpawnYSpawnear());
    }

    private System.Collections.IEnumerator DestruirAutoSpawnYSpawnear()
    {
        yield return null;
        yield return null;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            if (client.PlayerObject != null)
                client.PlayerObject.Despawn(true);

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            spawnHecho = true;
            SpawnJugadores();
        }
    }

    private void OnClientConectado(ulong clientId)
    {
        if (!IsServer || spawnHecho) return;

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            spawnHecho = true;
            SpawnJugadores();
        }
    }

    private void SpawnJugadores()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            bool esHost = client.ClientId == 0;
            GameObject prefab = esHost ? prefabHost : prefabClient;
            Transform punto = esHost ? spawner1 : spawner2;

            GameObject jugador = Instantiate(prefab, punto.position, punto.rotation);
            jugador.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            Debug.Log($"[SpawnManager] Jugador {client.ClientId} ({(esHost ? "HOST" : "CLIENT")}) spawneado en {punto.name}");
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConectado;
    }
}