using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class TagGame : NetworkBehaviour
{
    private MonoBehaviour miMovimiento;

    [Header("Configuración")]
    public float cooldownInicio = 3f;
    private float tiempoInicio;
    private bool juegoIniciado = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

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

        EliminarRpc(otroJugador.OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void EliminarRpc(ulong clientId)
    {
        DesactivarRpc(clientId);
        StartCoroutine(DespawnDespuesDeDelay(clientId));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DesactivarRpc(ulong clientId, RpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        if (miMovimiento != null)
            miMovimiento.enabled = false;

        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            if (mb.GetType().Name == "XRDeviceSimulator")
                mb.enabled = false;

        MostrarGameOver();
    }

    private void MostrarGameOver()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "CanvasGameOver")
            {
                canvas.gameObject.SetActive(true);
                return;
            }
        }
    }

    private void MostrarVictoria()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "CanvasWin")
            {
                canvas.gameObject.SetActive(true);
                Debug.Log("[Host] CanvasWin activado");
                return;
            }
        }
        Debug.LogWarning("[Host] CanvasWin no encontrado");
    }

    private IEnumerator DespawnDespuesDeDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var networkObject in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (networkObject.OwnerClientId == clientId && networkObject.IsPlayerObject)
            {
                GameObject objAEliminar = networkObject.gameObject;

                networkObject.Despawn(true);

                if (objAEliminar != null)
                    Destroy(objAEliminar);

                yield return new WaitForSeconds(0.1f);

                int clientesVivos = 0;
                foreach (var no in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
                    if (no.IsPlayerObject && no.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                        clientesVivos++;

                Debug.Log($"[Host] Clientes vivos: {clientesVivos}");

                if (clientesVivos == 0 && juegoIniciado)
                    MostrarVictoria();

                yield break;
            }
        }

        Debug.LogWarning($"[Servidor] No se encontró el objeto del cliente {clientId}");
    }
}