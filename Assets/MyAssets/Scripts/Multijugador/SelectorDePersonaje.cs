using Unity.Netcode;
using UnityEngine;

public class SelectorDePersonaje : MonoBehaviour
{
    public GameObject prefabHost;
    public GameObject prefabClient;

    public void IniciarComoHost()
    {
        // 1. Decimos qué prefab usar para el Host
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = prefabHost;
        // 2. Arrancamos
        NetworkManager.Singleton.StartHost();
    }

    public void IniciarComoClient()
    {
        // 1. Decimos qué prefab usar para el Cliente
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = prefabClient;
        // 2. Arrancamos
        NetworkManager.Singleton.StartClient();
    }
}