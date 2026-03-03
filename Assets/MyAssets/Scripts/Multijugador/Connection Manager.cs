using Unity.Netcode;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private bool started = false;

    private void Start()
    {
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        //NetworkManager.Singleton.SceneManager.LoadScene("Main_MP", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        if (started)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            started = true;
            StartHost();

        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            started = true;
            StartClient();
        }

    }
}
