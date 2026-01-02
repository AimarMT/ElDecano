using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCMovementCC : MonoBehaviour
{
    public float speed = 2f;
    public float tiempoParaNuevaDireccion = 4f;
    public float distanciaDeteccion = 1f;

    public float distanciaMinimaAlPlayer = 2f;
    public float fuerzaEvasion = 6f;

    CharacterController controller;
    Animator anim;

    float gravedad = -9.81f;
    float velocidadVertical = 0f;

    bool girando = false;
    Transform player;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (anim != null)
            anim.SetFloat("speed", speed);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        InvokeRepeating(nameof(NuevaDireccion), tiempoParaNuevaDireccion, tiempoParaNuevaDireccion);
    }

    void Update()
    {
        EvitarPlayerRadial();
        AplicarMovimiento();
        DetectarObstaculo();
    }

    void AplicarMovimiento()
    {
        if (controller.isGrounded)
            velocidadVertical = -1f;
        else
            velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = transform.forward * speed;
        movimiento.y = velocidadVertical;

        controller.Move(movimiento * Time.deltaTime);
    }

    void EvitarPlayerRadial()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float distancia = toPlayer.magnitude;

        if (distancia < distanciaMinimaAlPlayer)
        {
            Vector3 direccionHuida = -toPlayer.normalized;

            Quaternion rotObjetivo = Quaternion.LookRotation(direccionHuida);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotObjetivo,
                fuerzaEvasion * Time.deltaTime
            );

            girando = true;
            return;
        }

        girando = false;
    }

    void DetectarObstaculo()
    {
        if (girando) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, distanciaDeteccion))
        {
            girando = true;

            float giroAleatorio = Random.Range(120, 200);
            transform.Rotate(0, giroAleatorio, 0);

            Invoke(nameof(ResetGiro), 0.5f);
        }
    }

    void ResetGiro()
    {
        girando = false;
    }

    void NuevaDireccion()
    {
        if (girando) return;

        float giro = Random.Range(-120, 120);
        transform.Rotate(0, giro, 0);
    }
}
