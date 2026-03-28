using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MenuFlowManager : MonoBehaviour
{
    public static MenuFlowManager Instance { get; private set; }

    [Header("Escena del mapa")]
    public string mapSceneName = "MapaPillaPilla"; // Escribe el nombre directamente

    [Header("Mando")]
    [SerializeField] private GameObject mando;

    [Header("Colliders de botones")]
    [SerializeField] private Collider hostCollider;
    [SerializeField] private Collider clientCollider;
    [SerializeField] private Collider exitCollider;

    [Header("UI")]
    [SerializeField] private GameObject botonesGroup;
    [SerializeField] private TextMeshProUGUI textoEspera;

    private bool seleccionado = false;
    private bool escenaCargada = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        textoEspera.gameObject.SetActive(false);
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            Debug.Log($"[MenuFlowManager] Cliente conectado ID: {id}");
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
            Debug.Log($"[MenuFlowManager] Cliente desconectado ID: {id}");
    }

    void Update()
    {
        if (!seleccionado && mando != null)
        {
            if (hostCollider != null && hostCollider.bounds.Contains(mando.transform.position))
                SeleccionarHost();
            else if (clientCollider != null && clientCollider.bounds.Contains(mando.transform.position))
                SeleccionarClient();
            else if (exitCollider != null && exitCollider.bounds.Contains(mando.transform.position))
            {
                seleccionado = true;
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        if (!escenaCargada && seleccionado &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost &&
            NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            escenaCargada = true;
            Debug.Log("[MenuFlowManager] Cargando escena: " + mapSceneName);
            NetworkManager.Singleton.SceneManager.LoadScene(
                mapSceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void SeleccionarHost()
    {
        seleccionado = true;
        botonesGroup.SetActive(false);
        textoEspera.gameObject.SetActive(true);
        textoEspera.text = "Has escogido HOST\nEsperando al cliente...";
        NetworkManager.Singleton.StartHost();
    }

    private void SeleccionarClient()
    {
        seleccionado = true;
        botonesGroup.SetActive(false);
        textoEspera.gameObject.SetActive(true);
        textoEspera.text = "Has escogido CLIENT\nEsperando al host...";
        NetworkManager.Singleton.StartClient();
    }

    public void OnHostPressed() { if (!seleccionado) SeleccionarHost(); }
    public void OnClientPressed() { if (!seleccionado) SeleccionarClient(); }
}