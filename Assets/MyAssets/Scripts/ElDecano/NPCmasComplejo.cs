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
    public float rangoDeteccion = 10f;
    public float velocidadPatrulla = 2f;
    public float velocidadBusqueda = 3.5f;
    public float velocidadPersiguiendo = 5f;
    private float contadorBusqueda = 0f;
    public float tiempoBusqueda = 5f;

    private Vector3 ultimaPosicionJugador;
    public LayerMask capasObstaculos;

   
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Seguridad: desactivar/activar modelos según el estado inicial (Patrullando = normal)
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
        //Seguridad: si no hay jugador asignado, no intentes acceder a jugador.position
        if (jugador == null)
        {
            Debug.LogWarning("Jugador no asignado en NPCmasComplejo de " + name);
            
            return;
        }

        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        Debug.Log($"Estado: {estadoActual} | Velocidad: {agente.velocity.magnitude:F2}");

        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                if (distanciaJugador < rangoDeteccion)
                {
                    Vector3 origen = transform.position + Vector3.up * 1.5f;
                    Vector3 direccion = (jugador.position - origen).normalized;
                    RaycastHit hit;

                    int capasVisibles = LayerMask.GetMask("Player", "Obstaculo");
                    Debug.DrawRay(origen, direccion * rangoDeteccion, Color.red);

                    if (Physics.Raycast(origen, direccion, out hit, rangoDeteccion, capasVisibles))
                    {
                        if (hit.transform.CompareTag("Player"))
                        {
                            CambiarEstado(Estado.Persiguiendo);
                        }
                        else
                        {
                            Debug.Log("NO TE VEO, me tapa: " + hit.transform.name);
                        }
                    }
                }
                break;

            case Estado.Persiguiendo:
                Perseguir();

                Vector3 origenP = transform.position + Vector3.up * 1.5f;
                Vector3 direccionP = (jugador.position - origenP).normalized;
                RaycastHit hitP;

                int capasVisiblesP = LayerMask.GetMask("Player", "Obstaculo");
                bool veAlJugador = false;

                if (Physics.Raycast(origenP, direccionP, out hitP, rangoDeteccion, capasVisiblesP))
                {
                    if (hitP.transform.CompareTag("Player"))
                    {
                        veAlJugador = true;
                        ultimaPosicionJugador = jugador.position;
                    }
                }

                if (!veAlJugador)
                {
                    CambiarEstado(Estado.Buscando);
                }
                break;

            case Estado.Buscando:
                Buscar();
                break;
        }

        if (anim != null && agente != null)
            anim.SetFloat("Velocidad", agente.velocity.magnitude / Mathf.Max(1f, agente.speed));
    }

    void Patrullar()
    {
        agente.speed = velocidadPatrulla;
        if (!agente.pathPending && agente.remainingDistance < 0.3f)
        {
            puntoActual++;
            if (puntoActual >= rutas[rutaActual].puntos.Length)
                ElegirNuevaRuta();

            agente.destination = rutas[rutaActual].puntos[puntoActual].position;
        }
    }

    void Perseguir()
    {
        agente.destination = jugador.position;
        agente.speed = velocidadPersiguiendo;
    }

    void Buscar()
    {
        agente.speed = velocidadBusqueda;
        if (contadorBusqueda == 0f)
        {
            agente.destination = ultimaPosicionJugador;
        }

        contadorBusqueda += Time.deltaTime;

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            Vector3 randomPos = ultimaPosicionJugador + Random.insideUnitSphere * 3f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 3f, NavMesh.AllAreas))
                agente.destination = hit.position;
        }

        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        Vector3 origen = transform.position + Vector3.up * 1.5f;
        Vector3 direccion = (jugador.position - origen).normalized;
        RaycastHit hitInfo;

        int capasVisibles = LayerMask.GetMask("Player", "Obstaculo");

        if (Physics.Raycast(origen, direccion, out hitInfo, rangoDeteccion, capasVisibles))
        {
            if (hitInfo.transform.CompareTag("Player"))
            {
                CambiarEstado(Estado.Persiguiendo);
                agente.speed = velocidadPatrulla;
                return;
            }
        }

        if (contadorBusqueda >= tiempoBusqueda)
        {
            contadorBusqueda = 0f;
            CambiarEstado(Estado.Patrullando);
            agente.speed = velocidadPatrulla;
        }
    }

    void CambiarEstado(Estado nuevo)
    {
        estadoActual = nuevo;
        Debug.Log("Nuevo estado: " + nuevo);

        //Cambiar modelo según el estado
        if (nuevo == Estado.Persiguiendo || nuevo == Estado.Buscando)
        {
            CambiarModelo(true); //monstruo
        }
        else //Patrullando
        {
            CambiarModelo(false); //Modelo decano

            //Elige nueva ruta
            ElegirNuevaRuta();
        }
    }

    void CambiarModelo(bool monstruo)
{
    modeloNormal.SetActive(!monstruo);
    modeloMonstruo.SetActive(monstruo);

    if (monstruo)
        anim = modeloMonstruo.GetComponent<Animator>();
    else
        anim = modeloNormal.GetComponent<Animator>();

    anim.SetBool("EsMonstruo", monstruo);
}


    void ElegirNuevaRuta()
    {
        if (rutas == null || rutas.Length == 0) return;

        rutaActual = Random.Range(0, rutas.Length);
        puntoActual = 0;

        //Proteccion: si la ruta elegida no tiene puntos busca otra
        if (rutas[rutaActual].puntos == null || rutas[rutaActual].puntos.Length == 0)
        {
            Debug.LogWarning("Ruta sin puntos en NPCmasComplejo: " + name);
            return;
        }

        agente.destination = rutas[rutaActual].puntos[puntoActual].position;
    }
}
