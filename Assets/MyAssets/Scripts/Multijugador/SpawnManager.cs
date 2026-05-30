using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefabHost;
    [SerializeField] private GameObject prefabClient;
    [SerializeField] private Transform spawner1;
    [SerializeField] private Transform spawner2;

    private bool clienteSpawneado = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Destroy any auto-spawned player objects NGO may have created.
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
                Debug.Log($"[SpawnManager] Auto-spawn destruido para cliente {client.ClientId}");
            }
        }

        // Spawn the host immediately — no need to wait for the client.
        SpawnJugador(NetworkManager.Singleton.LocalClientId);

        // Wait for the client to finish loading, then spawn them.
        NetworkManager.SceneManager.OnLoadEventCompleted += OnEscenaCargadaCompletamente;
    }

    private void OnEscenaCargadaCompletamente(string sceneName, LoadSceneMode mode,
        List<ulong> clientesListos, List<ulong> clientesTimeout)
    {
        if (clienteSpawneado) return;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnEscenaCargadaCompletamente;

        Debug.Log($"[SpawnManager] Escena '{sceneName}' cargada en todos los clientes. Listos:{clientesListos.Count} Timeout:{clientesTimeout.Count}");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId) continue; // Host ya spawneado
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
                Debug.Log($"[SpawnManager] Player object existente destruido para cliente {client.ClientId}");
            }
            clienteSpawneado = true;
            SpawnJugador(client.ClientId);
        }
    }

    private void SpawnJugador(ulong clientId)
    {
        bool esHost = (clientId == NetworkManager.Singleton.LocalClientId);
        GameObject prefab = esHost ? prefabHost : prefabClient;
        Transform punto = esHost ? spawner1 : spawner2;

        GameObject jugador = Instantiate(prefab, punto.position, punto.rotation);
        jugador.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        Debug.Log($"[SpawnManager] Jugador {clientId} ({(esHost ? "HOST" : "CLIENT")}) spawneado en {punto.name} pos:{punto.position}");
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnEscenaCargadaCompletamente;
    }
}
