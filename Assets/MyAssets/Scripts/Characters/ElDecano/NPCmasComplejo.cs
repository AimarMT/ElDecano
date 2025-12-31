using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

[System.Serializable]
public class Ruta
{
    public Transform[] puntos;
}

public class NPCmasComplejo : MonoBehaviour
{
    public enum Estado { Patrullando, Buscando, Persiguiendo }
    public Estado estadoActual = Estado.Patrullando;

    [Header("Referencias")]
    public Transform jugador;

    [Header("Ataque y Muerte")]
    [Tooltip("Nombre de la escena que cargará al tocar al jugador")]
    public string nombreEscenaMuerte = "DeadScene"; 
    [Tooltip("Distancia mínima para matar si no usas triggers físicos")]
    public float distanciaParaMatar = 1.5f;

    [Header("Rutas")]
    public Ruta[] rutas;

    [Header("Modelos")]
    public GameObject modeloNormal;
    public GameObject modeloMonstruo;

    private int rutaActual = 0;
    private int puntoActual = 0;

    private NavMeshAgent agente;
    private Animator anim;

    [Header("Detección y velocidades")]
    public float rangoDeteccion = 15f; 
    public float velocidadPatrulla = 2f;
    public float velocidadBusqueda = 3.5f;
    public float velocidadPersiguiendo = 5f;
    public float tiempoBusqueda = 5f;
    private float contadorBusqueda = 0f;

    private Vector3 ultimaPosicionJugador;

    [Header("Filtros de Visión")]
    public LayerMask capasVisibles;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        CambiarModelo(false);

        if (rutas == null || rutas.Length == 0)
        {
            Debug.LogWarning("No hay rutas asignadas a NPCmasComplejo en " + name);
            enabled = false;
            return;
        }

        ElegirNuevaRuta();
    }

    // --- DETECCIÓN DE CONTACTO ---
    // Este método se activa si el enemigo tiene un Collider y el jugador otro.
    // Uno de los dos debe tener un Rigidbody.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AtacarJugador();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AtacarJugador();
        }
    }

    void AtacarJugador()
    {
        Debug.Log("¡El Decano te ha atrapado!");
        SceneManager.LoadScene(nombreEscenaMuerte);
    }

    void Update()
    {
        if (jugador == null) return;

        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);

        // Seguridad extra: Si por alguna razón el trigger falla pero está encima del jugador
        if (distanciaJugador < distanciaParaMatar && estadoActual == Estado.Persiguiendo)
        {
            AtacarJugador();
        }

        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                if (distanciaJugador < rangoDeteccion && TieneVisionDirecta())
                {
                    CambiarEstado(Estado.Persiguiendo);
                }
                break;

            case Estado.Persiguiendo:
                Perseguir();
                if (TieneVisionDirecta())
                {
                    ultimaPosicionJugador = jugador.position;
                }
                else
                {
                    CambiarEstado(Estado.Buscando);
                }
                break;

            case Estado.Buscando:
                Buscar();
                if (distanciaJugador < rangoDeteccion && TieneVisionDirecta())
                {
                    CambiarEstado(Estado.Persiguiendo);
                }
                break;
        }

        ActualizarAnimaciones();
    }

    // (El resto de funciones se mantienen igual que en tu script original...)
    bool TieneVisionDirecta()
    {
        Vector3 origen = transform.position + Vector3.up * 1.6f;
        Vector3 objetivo = jugador.position + Vector3.up * 1.3f;
        Vector3 direccion = (objetivo - origen).normalized;
        RaycastHit hit;

        if (Physics.Raycast(origen, direccion, out hit, rangoDeteccion, capasVisibles))
        {
            if (hit.transform.CompareTag("Player")) return true;
        }
        return false;
    }

    void Patrullar()
    {
        agente.speed = velocidadPatrulla;
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            puntoActual++;
            if (puntoActual >= rutas[rutaActual].puntos.Length) ElegirNuevaRuta();
            else agente.destination = rutas[rutaActual].puntos[puntoActual].position;
        }
    }

    void Perseguir() { agente.speed = velocidadPersiguiendo; agente.destination = jugador.position; }

    void Buscar()
    {
        agente.speed = velocidadBusqueda;
        if (contadorBusqueda == 0f) agente.destination = ultimaPosicionJugador;
        contadorBusqueda += Time.deltaTime;

        if (!agente.pathPending && agente.remainingDistance < 0.6f)
        {
            Vector3 randomPos = ultimaPosicionJugador + Random.insideUnitSphere * 4f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPos, out navHit, 4f, NavMesh.AllAreas))
                agente.destination = navHit.position;
        }

        if (contadorBusqueda >= tiempoBusqueda)
        {
            contadorBusqueda = 0f;
            CambiarEstado(Estado.Patrullando);
        }
    }

    void CambiarEstado(Estado nuevo)
    {
        if (estadoActual == nuevo) return;
        estadoActual = nuevo;
        if (nuevo == Estado.Persiguiendo || nuevo == Estado.Buscando) CambiarModelo(true);
        else { CambiarModelo(false); ElegirNuevaRuta(); }
    }

    void CambiarModelo(bool monstruo)
    {
        if (modeloNormal != null) modeloNormal.SetActive(!monstruo);
        if (modeloMonstruo != null) modeloMonstruo.SetActive(monstruo);
        anim = monstruo ? modeloMonstruo.GetComponent<Animator>() : modeloNormal.GetComponent<Animator>();
        if (anim != null) anim.SetBool("EsMonstruo", monstruo);
    }

    void ActualizarAnimaciones()
    {
        if (anim != null && agente != null)
        {
            float v = agente.velocity.magnitude / agente.speed;
            anim.SetFloat("Velocidad", v);
        }
    }

    void ElegirNuevaRuta()
    {
        if (rutas == null || rutas.Length == 0) return;
        rutaActual = Random.Range(0, rutas.Length);
        puntoActual = 0;
        if(rutas[rutaActual].puntos.Length > 0)
            agente.destination = rutas[rutaActual].puntos[puntoActual].position;
    }
}