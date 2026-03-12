using UnityEngine;
using TMPro;

public class CronometroPillaPilla : MonoBehaviour
{
    public TextMeshProUGUI textoReloj;

    [Header("Colores")]
    public Color colorNormal = Color.green;
    public Color colorAlerta = Color.red;

    private float tiempoDeInicioInterno;
    private float tiempoActual;

    void Awake()
    {
        tiempoDeInicioInterno = Time.time;
    }

    void Update()
    {
        tiempoActual = Time.time - tiempoDeInicioInterno;

        // Formateo de tiempo
        int minutos = Mathf.FloorToInt(tiempoActual / 60);
        int segundos = Mathf.FloorToInt(tiempoActual % 60);

        if (textoReloj != null)
        {
            textoReloj.text = string.Format("{0:00}:{1:00}", minutos, segundos);

            // LÓGICA DEL COLOR:
            if (tiempoActual >= 10f)
            {
                textoReloj.color = colorAlerta; // Se pone rojo
            }
            else
            {
                textoReloj.color = colorNormal; // Sigue verde
            }
        }
    }

    // Esta es la función que el Spawner busca para no dar error
    public float GetTiempoInicio()
    {
        return tiempoDeInicioInterno;
    }
}