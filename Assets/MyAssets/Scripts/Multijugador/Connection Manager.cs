using Unity.Netcode;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    // --- SINGLETON ---
    public static ConnectionManager Instance { get; private set; }

    private bool started = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() { }

    public void StartHost()
    {
        if (started) return;
        started = true;
        NetworkManager.Singleton.StartHost();
        Debug.Log("[ConnectionManager] Host iniciado.");
        // NetworkManager.Singleton.SceneManager.LoadScene("Main_MP", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void StartClient()
    {
        if (started) return;
        started = true;
        NetworkManager.Singleton.StartClient();
        Debug.Log("[ConnectionManager] Client iniciado.");
    }

    void Update()
    {
        // Atajo de teclado para pruebas en Editor (sin VR)
        if (started) return;

        if (Input.GetKeyDown(KeyCode.H)) StartHost();
        if (Input.GetKeyDown(KeyCode.C)) StartClient();
    }
}