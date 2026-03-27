using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class OPenScene : MonoBehaviour
{
    [Header("Escena a cargar")]
    [SerializeField] private Object mapScene;

    public void OnHostSelected()
    {
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene(mapScene.name);
        Debug.Log("carganod host");
    }

    public void OnClientSelected()
    {
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene(mapScene.name);
        Debug.Log("carganod client");
    }
}