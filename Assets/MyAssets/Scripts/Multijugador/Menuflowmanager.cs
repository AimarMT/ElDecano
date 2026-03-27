using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MenuFlowManager : MonoBehaviour
{
    public static MenuFlowManager Instance { get; private set; }

    [Header("Escena del mapa")]
    [SerializeField] private Object mapScene;

    [Header("Mando")]
    [SerializeField] private GameObject mando;

    [Header("Colliders de botones")]
    [SerializeField] private Collider hostCollider;
    [SerializeField] private Collider clientCollider;
    [SerializeField] private Collider exitCollider;

    private bool seleccionado = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Update()
    {
        if (seleccionado) return;
        if (mando == null) return;

        if (hostCollider != null && hostCollider.bounds.Contains(mando.transform.position))
        {
            Debug.Log("[MenuFlowManager] HOST seleccionado");
            seleccionado = true;
            NetworkManager.Singleton.StartHost();
            SceneManager.LoadScene(mapScene.name);
        }
        else if (clientCollider != null && clientCollider.bounds.Contains(mando.transform.position))
        {
            Debug.Log("[MenuFlowManager] CLIENT seleccionado");
            seleccionado = true;
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene(mapScene.name);
        }
        else if (exitCollider != null && exitCollider.bounds.Contains(mando.transform.position))
        {
            Debug.Log("[MenuFlowManager] EXIT seleccionado");
            seleccionado = true;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // Estos métodos los mantengo por si los botones UI siguen conectados
    public void OnHostPressed()
    {
        if (seleccionado) return;
        seleccionado = true;
        Debug.Log("[MenuFlowManager] HOST seleccionado");
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene(mapScene.name);
    }

    public void OnClientPressed()
    {
        if (seleccionado) return;
        seleccionado = true;
        Debug.Log("[MenuFlowManager] CLIENT seleccionado");
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene(mapScene.name);
    }
}