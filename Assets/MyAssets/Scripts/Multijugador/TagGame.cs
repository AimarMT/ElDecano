using Unity.Netcode;
using UnityEngine;

public class TagGame : NetworkBehaviour
{
    private GameObject gameOverPanel;
    private MonoBehaviour miMovimiento;

    [Header("Configuración")]
    public float cooldownInicio = 3f; 
    private float tiempoInicio;
    private bool juegoIniciado = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (IsOwner)
        {
            GameObject move = GameObject.Find("Move");
            if (move != null)
                miMovimiento = move.GetComponent<MonoBehaviour>();
            else
                Debug.LogWarning("[TagGame] No se encontró el objeto Move");
        }

        tiempoInicio = Time.time;
    }

    private void Update()
    {
        if (!juegoIniciado && Time.time - tiempoInicio >= cooldownInicio)
        {
            juegoIniciado = true;
            Debug.Log("[TagGame] ¡Juego iniciado!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost) return;
        if (!juegoIniciado) return; 

        TagGame otroJugador = other.GetComponentInParent<TagGame>();
        if (otroJugador == null) return;
        if (otroJugador.OwnerClientId == NetworkManager.Singleton.LocalClientId) return;

        EliminarServerRpc(otroJugador.OwnerClientId);
    }

    [ServerRpc]
    private void EliminarServerRpc(ulong clientId)
    {
        DesactivarClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });

        StartCoroutine(DespawnDespuesDeDelay(clientId));
    }

    [ClientRpc]
    private void DesactivarClientRpc(ClientRpcParams rpcParams = default)
    {
        if (miMovimiento != null)
            miMovimiento.enabled = false;

        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            if (mb.GetType().Name == "XRDeviceSimulator")
                mb.enabled = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("Has sido eliminado.");
    }

    private System.Collections.IEnumerator DespawnDespuesDeDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            client.PlayerObject.Despawn(true);
    }
}