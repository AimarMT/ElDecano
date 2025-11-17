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

    public Transform jugador;
    public Ruta[] rutas;

    private int rutaActual = 0;
    private int puntoActual = 0;

    private NavMeshAgent agente;
    private Animator anim;

    public float rangoDeteccion = 10f;
    

    public float velocidadPatrulla = 2f;  //velocidad normal
    public float velocidadBusqueda = 3.5f;    //velocidad mientras busca
    public float velocidadPersiguiendo = 5f; //velocidad mientras persigue
    private float contadorBusqueda = 0f;    //contador de tiempo en busqueda
    public float tiempoBusqueda = 5f;       //tiempo total que dura buscando


    private Vector3 ultimaPosicionJugador;

    public LayerMask capasObstaculos;//Esta sera la layerMask que marcara que "3D object"s son Obstaculos para que el NPC no vea a tarves.



    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        ElegirNuevaRuta();
    }

    void Update()
    {
        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        Debug.Log($"Estado: {estadoActual} | Velocidad: {agente.velocity.magnitude:F2}"); //Estado del Decano | Velocidad

        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                if (distanciaJugador < rangoDeteccion)
                {
                    Vector3 origen = transform.position + Vector3.up * 1.5f;
                    Vector3 direccion = (jugador.position - origen).normalized;
                    RaycastHit hit;

                    int capasVisibles = LayerMask.GetMask("Player", "Obstaculo"); //Las capas necesarias para el juego ("Player" - Para el jugador | "Obstaculo" para todo aque objeto que bloquee la vision)
                    Debug.DrawRay(origen, direccion * rangoDeteccion, Color.red);//Dibuja el raycast

                    if (Physics.Raycast(origen, direccion, out hit, rangoDeteccion, capasVisibles))
                    {
           

                        if (hit.transform.CompareTag("Player")) //Compara el tag del GameObject con el que ha golpeado
                        {
                            CambiarEstado(Estado.Persiguiendo); //Cambia de estado
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

                // Reusa variables con nombres distintos para no chocar
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
                        ultimaPosicionJugador = jugador.position; //guarda la ultima posicion visible
                    }
                }

                // si deja de verlo
                if (!veAlJugador)
                {
                    CambiarEstado(Estado.Buscando);
                }


            break;

            case Estado.Buscando:
                Buscar();
                break;
        }

        anim.SetFloat("Velocidad", agente.velocity.magnitude / agente.speed);
    }

    void Patrullar()
    {
        agente.speed = velocidadPatrulla; //Asigna la velocidad dependiendo de el estado
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
        agente.speed = velocidadPersiguiendo; //Asigna la velocidad dependiendo de el estado
    }

    void Buscar()
    {
        // Setea velocidad más alta para búsqueda
        agente.speed = velocidadBusqueda;
        if (contadorBusqueda == 0)
        {
            // Ir hacia la última posición conocida del jugador
            agente.destination = ultimaPosicionJugador;
        }

        contadorBusqueda += Time.deltaTime;

        // Simula movimiento aleatorio cerca de la última posición
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            Vector3 randomPos = ultimaPosicionJugador + Random.insideUnitSphere * 3f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 3f, NavMesh.AllAreas))
                agente.destination = hit.position;
        }

        //si encuentra al jugador nuevamente, vuelve a perseguir
        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        Vector3 origen = transform.position + Vector3.up * 1.5f;
        Vector3 direccion = (jugador.position - origen).normalized;
        RaycastHit hitInfo;

        int capasVisibles = LayerMask.GetMask("Player", "Obstaculo"); //coge las capas

        if (Physics.Raycast(origen, direccion, out hitInfo, rangoDeteccion, capasVisibles))
        {
            if (hitInfo.transform.CompareTag("Player"))
            {
                CambiarEstado(Estado.Persiguiendo);
                agente.speed = velocidadPatrulla; //resetea velocidad
                return;
            }
        }

        //si termina el tiempo de busqueda, vuelve a patrullar
        if (contadorBusqueda >= tiempoBusqueda)
        {
            contadorBusqueda = 0f;
            CambiarEstado(Estado.Patrullando);
            agente.speed = velocidadPatrulla; //vuelve a velocidad normal
        }
    }

    void CambiarEstado(Estado nuevo)
    {
        estadoActual = nuevo;
        Debug.Log("Nuevo estado: " + nuevo);
    }

    void ElegirNuevaRuta()
    {
        rutaActual = Random.Range(0, rutas.Length);
        puntoActual = 0;
        agente.destination = rutas[rutaActual].puntos[puntoActual].position;
    }
}
