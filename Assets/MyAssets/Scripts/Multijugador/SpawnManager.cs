using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefabHost;
    [SerializeField] private GameObject prefabClient;
    [SerializeField] private Transform spawner1;
    [SerializeField] private Transform spawner2;

    private bool spawnHecho = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectado;
        StartCoroutine(DestruirAutoSpawnYSpawnear());
    }

    private IEnumerator DestruirAutoSpawnYSpawnear()
    {
        spawnHecho = true; // Bloquear antes de los yields

        yield return null;
        yield return null;

        int destruidos = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
                destruidos++;
            }
        }
        Debug.Log($"[SpawnManager] Auto-spawn destruido: {destruidos} player objects eliminados");

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            SpawnJugadores();
        }
        else
        {
            Debug.Log($"[SpawnManager] Solo {NetworkManager.Singleton.ConnectedClients.Count} cliente(s), esperando al segundo...");
            spawnHecho = false; // Permitir que OnClientConectado actúe
        }
    }

    private void OnClientConectado(ulong clientId)
    {
        if (!IsServer || spawnHecho) return;
        Debug.Log($"[SpawnManager] Cliente {clientId} conectado. Total: {NetworkManager.Singleton.ConnectedClients.Count}");

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            spawnHecho = true;
            SpawnJugadores();
        }
    }

    private void SpawnJugadores()
    {
        Debug.Log($"[SpawnManager] Spawneando jugadores. Clientes: {NetworkManager.Singleton.ConnectedClients.Count}");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                Debug.LogWarning($"[SpawnManager] Cliente {client.ClientId} aún tiene PlayerObject, destruyendo...");
                client.PlayerObject.Despawn(true);
            }

            bool esHost = client.ClientId == 0;
            GameObject prefab = esHost ? prefabHost : prefabClient;
            Transform punto = esHost ? spawner1 : spawner2;

            GameObject jugador = Instantiate(prefab, punto.position, punto.rotation);
            jugador.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);

            Debug.Log($"[SpawnManager] Jugador {client.ClientId} ({(esHost ? "HOST" : "CLIENT")}) spawneado en {punto.name} pos:{punto.position}");
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConectado;
    }
}