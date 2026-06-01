using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class TagGame : NetworkBehaviour
{
    private HandGestureMovement miMovimiento;

    [Header("Configuración")]
    public float cooldownInicio = 3f;
    public float catchDistance = 2f;
    private float tiempoInicio;
    private bool juegoIniciado = false;
    private bool capturaEnProgreso = false;
    private int _distFrame = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            XRRigReferences2[] rigs = FindObjectsByType<XRRigReferences2>(FindObjectsSortMode.None);
            foreach (var rig in rigs)
            {
                Camera cam = rig.head?.GetComponent<Camera>();
                if (cam != null && cam.isActiveAndEnabled)
                {
                    miMovimiento = rig.root?.GetComponent<HandGestureMovement>();
                    break;
                }
            }
            if (miMovimiento == null)
                Debug.LogWarning("[TagGame] No se encontró HandGestureMovement en el rig local");
        }

        tiempoInicio = Time.time;
    }

    private void Update()
    {
        if (!juegoIniciado && Time.time - tiempoInicio >= cooldownInicio)
        {
            juegoIniciado = true;
            Debug.Log($"[TagGame] ¡Juego iniciado! IsHost={IsHost} IsOwner={IsOwner} pos={transform.position:F2}");
        }

        if (!IsHost || !IsOwner || !juegoIniciado || capturaEnProgreso) return;

        _distFrame++;

        // Busca por NetworkObject.IsPlayerObject para no depender de que
        // el prefab del cliente tenga TagGame.
        foreach (var netObj in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (!netObj.IsPlayerObject) continue;
            if (netObj.OwnerClientId == NetworkManager.Singleton.LocalClientId) continue;

            float dist = Vector3.Distance(transform.position, netObj.transform.position);

            if (_distFrame % 180 == 0)
                Debug.Log($"[TagGame] Dist a cliente {netObj.OwnerClientId}: {dist:F2}m (umbral={catchDistance}) miPos={transform.position:F2} suPos={netObj.transform.position:F2}");

            if (dist <= catchDistance)
            {
                capturaEnProgreso = true;
                Debug.Log($"[TagGame] ¡Captura! dist={dist:F2}m clientId={netObj.OwnerClientId}");
                ExitCube exitCube = FindAnyObjectByType<ExitCube>();
                if (exitCube != null)
                    exitCube.HostKillPlayerRpc(netObj.OwnerClientId);
                else
                    Debug.LogWarning("[TagGame] ExitCube no encontrado para la captura");
                break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void EliminarRpc()
    {
        Debug.Log($"[TagGame] EliminarRpc recibido para clientId={OwnerClientId}");
        ExitCube exitCube = FindAnyObjectByType<ExitCube>();
        if (exitCube != null)
        {
            Debug.Log($"[TagGame] ExitCube encontrado → HostKillPlayerRpc({OwnerClientId})");
            exitCube.HostKillPlayerRpc(OwnerClientId);
        }
        else
        {
            Debug.LogWarning($"[TagGame] ExitCube no encontrado para clientId={OwnerClientId}, usando fallback");
            DesactivarRpc(RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            StartCoroutine(DespawnDespuesDeDelay(OwnerClientId));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DesactivarRpc(RpcParams rpcParams = default)
    {
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