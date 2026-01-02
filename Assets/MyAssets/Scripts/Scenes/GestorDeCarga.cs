using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorDeCarga : MonoBehaviour
{
    void Start()
    {
        // En cuanto aparece la escena de carga, empieza a traer la universidad
        StartCoroutine(CargaRealAsincrona());
    }

    IEnumerator CargaRealAsincrona()
    {
        // Esperamos un segundo para que el usuario vea el texto "CARGANDO" 
        // y el visor VR se estabilice.
        yield return new WaitForSeconds(1.5f);

        AsyncOperation operacion = SceneManager.LoadSceneAsync("UniversityMap");
        
        // Esto asegura que cuando termine de cargar, cambie automáticamente
        operacion.allowSceneActivation = true;

        while (!operacion.isDone)
        {
            yield return null;
        }
    }
}