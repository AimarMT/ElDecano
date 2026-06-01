using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitCube : NetworkBehaviour
{
    [Header("Escenas")]
    public string clientExitScene = "WinMenuMultiplayerCliente";
    public string clientDeadScene = "GameOverMenuMultiplayerCliente";
    public string hostWinScene = "WinMenuMultiplayerHost";
    public string hostExitScene = "GameOverMenuMultiplayerHost";

    private NetworkVariable<int> clientCount = new NetworkVariable<int>(0);
    private NetworkVariable<int> deadCount = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            clientCount.Value = 0;
            deadCount.Value = 0;
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (kvp.Key != NetworkManager.ServerClientId)
                    clientCount.Value++;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        clientCount.OnValueChanged += OnClientCountChanged;
        deadCount.OnValueChanged += OnDeadCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        clientCount.OnValueChanged -= OnClientCountChanged;
        deadCount.OnValueChanged -= OnDeadCountChanged;
        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.ServerClientId)
            clientCount.Value++;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.ServerClientId)
            clientCount.Value = Mathf.Max(0, clientCount.Value - 1);
    }

    private void OnClientCountChanged(int oldValue, int newValue)
    {
        Debug.Log($"[Host] Clientes restantes: {newValue}");

        if (newValue <= 0 && IsServer)
        {
            if (deadCount.Value == 0)
            {
                // El cliente completó el puzzle y salió → el host pierde
                Debug.Log("[Host] Cliente completó el puzzle. Cargando GameOver del host.");
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene(hostExitScene);
            }
            // Si deadCount > 0, el host pilló al cliente → DisconnectAndCheckWin
            // carga WinMenuMultiplayerHost por su cuenta, aquí no hacemos nada.
        }
    }

    private void OnDeadCountChanged(int oldValue, int newValue)
    {
        Debug.Log($"[Host] Clientes muertos: {newValue}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            return;

        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return;

        RequestExitRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestExitRpc(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId) return;

        LoadClientSceneRpc(clientExitScene, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        StartCoroutine(DisconnectAfterDelay(clientId));
    }

    [Rpc(SendTo.Server)]
    public void HostKillPlayerRpc(ulong victimClientId)
    {
        Debug.Log($"[Host] HostKillPlayerRpc: victimId={victimClientId}");
        if (victimClientId == NetworkManager.ServerClientId) return;

        deadCount.Value++;
        Debug.Log($"[Host] Cargando escena '{clientDeadScene}' en cliente {victimClientId}");
        LoadClientSceneRpc(clientDeadScene, RpcTarget.Single(victimClientId, RpcTargetUse.Temp));
        StartCoroutine(DisconnectAndCheckWin(victimClientId));
    }

    private IEnumerator DisconnectAndCheckWin(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }

        yield return new WaitForSeconds(0.3f);

        int alive = 0;
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            if (kvp.Key != NetworkManager.ServerClientId)
                alive++;
        }

        if (alive <= 0)
        {
            Debug.Log("[Host] Todos muertos. Victoria!");
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(hostWinScene);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void LoadClientSceneRpc(string sceneName, RpcParams rpcParams = default)
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DisconnectAfterDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
}