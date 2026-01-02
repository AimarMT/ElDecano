using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeadScene : MonoBehaviour
{
    [SerializeField] private float tiempoDeEspera = 3f;
    [SerializeField] private string nombreEscenaGameOver = "GameOver";

    void Start()
    {
        // Iniciamos la cuenta atrás en cuanto carga la escena
        StartCoroutine(EsperarYCambiarEscena());
    }

    IEnumerator EsperarYCambiarEscena()
    {
        //Espera la cantidad de segundos definida
        yield return new WaitForSeconds(tiempoDeEspera);
        SceneManager.LoadScene(nombreEscenaGameOver);
        Debug.Log("Cargando escena" + nombreEscenaGameOver);
    }
}