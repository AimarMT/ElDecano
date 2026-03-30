using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawner1;
    [SerializeField] private Transform spawner2;

    private bool spawnHecho = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectado;

        // Destruir los player objects spawneados automáticamente
        StartCoroutine(DestruirAutoSpawnYSpawnear());
    }

    private System.Collections.IEnumerator DestruirAutoSpawnYSpawnear()
    {
        // Esperar un frame para que Netcode termine de spawnear los automáticos
        yield return null;
        yield return null;

        // Destruir todos los player objects actuales
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
            }
        }

        // Si ya hay 2 jugadores, spawnear directamente
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