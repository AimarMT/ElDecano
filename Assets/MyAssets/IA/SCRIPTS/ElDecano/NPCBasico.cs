using UnityEngine;
using UnityEngine.AI;

public class NPCPatrullaAnim : MonoBehaviour
{
    public Transform[] puntos;
    private int indiceActual = 0;

    private NavMeshAgent agente;
    private Animator anim;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Configurar NavMeshAgent
        agente.speed = 1.5f;                // velocidad del NPC
        agente.stoppingDistance = 0.3f;   // distancia para considerar que llegó
        agente.autoBraking = false;       // para que la velocidad sea constante

        // Empezar con el primer punto
        if (puntos.Length > 0)
            agente.destination = puntos[indiceActual].position;
    }

    void Update()
    {
        // Normalizar la velocidad para el Animator (0 = Idle, 1 = velocidad máxima)
        float velocidadAnim = agente.velocity.magnitude / agente.speed;
        anim.SetFloat("Velocidad", velocidadAnim);

        // Cambiar de punto al llegar
        if (!agente.pathPending && agente.remainingDistance < agente.stoppingDistance)
        {
            indiceActual = (indiceActual + 1) % puntos.Length;
            agente.destination = puntos[indiceActual].position;
        }
    }
}
