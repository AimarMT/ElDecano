using UnityEngine;
using UnityEngine.AI;

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
    [Tooltip("Asegúrate de marcar 'Player' y 'Obstaculo' (o Default) en el Inspector")]
    public LayerMask capasVisibles;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        
        // Inicializar modelo
        CambiarModelo(false);

        if (rutas == null || rutas.Length == 0)
        {
            Debug.LogWarning("No hay rutas asignadas a NPCmasComplejo en " + name);
            enabled = false;
            return;
        }

        ElegirNuevaRuta();
    }

    void Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("¡Asigna el XR Origin al script del Decano!");
            return;
        }

        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                // Si está cerca y hay visión, persigue
                if (distanciaJugador < rangoDeteccion && TieneVisionDirecta())
                {
                    CambiarEstado(Estado.Persiguiendo);
                }
                break;

            case Estado.Persiguiendo:
                Perseguir();
                // Mientras lo vea, actualizo la última posición conocida
                if (TieneVisionDirecta())
                {
                    ultimaPosicionJugador = jugador.position;
                }
                else
                {
                    // Si lo pierde de vista, pasa a buscar
                    CambiarEstado(Estado.Buscando);
                }
                break;

            case Estado.Buscando:
                Buscar();
                // Si durante la búsqueda lo vuelve a ver, persigue otra vez
                if (distanciaJugador < rangoDeteccion && TieneVisionDirecta())
                {
                    CambiarEstado(Estado.Persiguiendo);
                }
                break;
        }

        ActualizarAnimaciones();
    }

    // LÓGICA DE VISIÓN MEJORADA PARA VR
    bool TieneVisionDirecta()
    {
        // Ojos del Decano (ajustado a 1.6m de altura)
        Vector3 origen = transform.position + Vector3.up * 1.6f;
        // Objetivo: El pecho/cabeza del jugador (1.3m de altura del XR Origin)
        Vector3 objetivo = jugador.position + Vector3.up * 1.3f;
        
        Vector3 direccion = (objetivo - origen).normalized;
        RaycastHit hit;

        // Dibujar rayo en el editor para debuguear
        Debug.DrawRay(origen, direccion * rangoDeteccion, Color.red);

        if (Physics.Raycast(origen, direccion, out hit, rangoDeteccion, capasVisibles))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void Patrullar()
    {
        agente.speed = velocidadPatrulla;
        
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            puntoActual++;
            if (puntoActual >= rutas[rutaActual].puntos.Length)
            {
                ElegirNuevaRuta();
            }
            else
            {
                agente.destination = rutas[rutaActual].puntos[puntoActual].position;
            }
        }
    }

    void Perseguir()
    {
        agente.speed = velocidadPersiguiendo;
        agente.destination = jugador.position;
    }

    void Buscar()
    {
        agente.speed = velocidadBusqueda;

        // Ir a la última posición donde vio al jugador
        if (contadorBusqueda == 0f)
            agente.destination = ultimaPosicionJugador;

        contadorBusqueda += Time.deltaTime;

        // Si llega a la última posición y no lo ve, da vueltas por la zona
        if (!agente.pathPending && agente.remainingDistance < 0.6f)
        {
            Vector3 randomPos = ultimaPosicionJugador + Random.insideUnitSphere * 4f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPos, out navHit, 4f, NavMesh.AllAreas))
                agente.destination = navHit.position;
        }

        // Si pasa el tiempo y no lo encuentra, vuelve a patrullar
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
        Debug.Log("El Decano ahora está: " + nuevo);

        if (nuevo == Estado.Persiguiendo || nuevo == Estado.Buscando)
        {
            CambiarModelo(true); // Se transforma en monstruo
        }
        else
        {
            CambiarModelo(false); // Vuelve a ser el decano normal
            ElegirNuevaRuta();
        }
    }

    void CambiarModelo(bool monstruo)
    {
        if (modeloNormal != null) modeloNormal.SetActive(!monstruo);
        if (modeloMonstruo != null) modeloMonstruo.SetActive(monstruo);

        // Actualizamos la referencia del animator al modelo que esté activo
        anim = monstruo ? modeloMonstruo.GetComponent<Animator>() : modeloNormal.GetComponent<Animator>();
        
        if (anim != null)
            anim.SetBool("EsMonstruo", monstruo);
    }

    void ActualizarAnimaciones()
    {
        if (anim != null && agente != null)
        {
            // Calcula la velocidad para las animaciones (0 quieto, 1 corriendo)
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