using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Ajustes de Spawn")]
    public GameObject prefabJugador;
    public GameObject prefabEnemigo;

    public Transform puntoSpawnJugador;
    public Transform puntoSpawnEnemigo;

    [Header("Referencia al Cronometro")]
    public CronometroPillaPilla cronometro; // Aquí arrastraremos el objeto que tiene el script del reloj

    private bool enemigoSpawneado = false;

    void Start()
    {
        // Al empezar, spawneamos al jugador inmediatamente
        SpawnJugador();
    }

    void Update()
    {
        // Comprobamos si el tiempo ha llegado a 10 y si no hemos spawneado al enemigo aún
        // Nota: Accedemos a 'tiempoTranscurrido' que es el tiempo que calculamos en el otro script
        if (cronometro != null && Time.time - cronometro.GetTiempoInicio() >= 10f && !enemigoSpawneado)
        {
            SpawnEnemigo();
            enemigoSpawneado = true;
        }
    }

    void SpawnJugador()
    {
        if (prefabJugador != null && puntoSpawnJugador != null)
        {
            Instantiate(prefabJugador, puntoSpawnJugador.position, puntoSpawnJugador.rotation);
            Debug.Log("Jugador spawneado. ¡Tienes 10 segundos de ventaja!");
        }
    }

    void SpawnEnemigo()
    {
        if (prefabEnemigo != null && puntoSpawnEnemigo != null)
        {
            Instantiate(prefabEnemigo, puntoSpawnEnemigo.position, puntoSpawnEnemigo.rotation);
            Debug.Log("¡El enemigo ha aparecido! Comienza la caza.");
        }
    }
}