using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Pon este script en CADA socket (el de Host y el de Client).
/// En el Inspector marca si es Host o Client con el enum.
/// </summary>
public class HostClientSocket : MonoBehaviour
{
    public enum RoleType { Host, Client }

    [Header("Rol de este socket")]
    public RoleType role;

    [Header("Canvas de selección de prefab (asígnalo aquí)")]
    public GameObject prefabSelectionCanvas;

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnObjectDropped);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnObjectDropped);
    }

    private void OnObjectDropped(SelectEnterEventArgs args)
    {
        // 1. Iniciamos conexión según el rol del socket
        if (role == RoleType.Host)
        {
            Debug.Log("[HostClientSocket] Iniciando como HOST");
            ConnectionManager.Instance.StartHost();
        }
        else
        {
            Debug.Log("[HostClientSocket] Iniciando como CLIENT");
            ConnectionManager.Instance.StartClient();
        }

        // 2. Mostramos el canvas de selección de prefab
        if (prefabSelectionCanvas != null)
        {
            prefabSelectionCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[HostClientSocket] No hay canvas asignado en el Inspector.");
        }
    }
}