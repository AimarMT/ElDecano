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

        // No spawnear todavía — esperar a que la escena esté cargada en TODOS los clientes.
        // Si el host spawna aquí, el NGO sincroniza el avatar al cliente mientras la escena
        // del menú sigue activa; cuando el menú se destruye, el avatar se destruye con él.
        NetworkManager.SceneManager.OnLoadEventCompleted += OnEscenaCargadaCompletamente;
    }

    private void OnEscenaCargadaCompletamente(string sceneName, LoadSceneMode mode,
        List<ulong> clientesListos, List<ulong> clientesTimeout)
    {
        if (clienteSpawneado) return;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnEscenaCargadaCompletamente;

        Debug.Log($"[SpawnManager] Escena '{sceneName}' lista en todos los clientes. Listos:{clientesListos.Count} Timeout:{clientesTimeout.Count}");

        // Ahora la escena del mapa está activa en TODOS los clientes.
        // Spawnear host Y cliente aquí garantiza que los avatares se crean
        // en la escena correcta y no se destruyen al descargar el menú.
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true);
                Debug.Log($"[SpawnManager] PlayerObject existente destruido para cliente {client.ClientId}");
            }
            SpawnJugador(client.ClientId);
        }

        clienteSpawneado = true;
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
