using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCMovementCC : MonoBehaviour
{
    public float speed = 2f;
    public float tiempoParaNuevaDireccion = 4f;
    public float distanciaDeteccion = 1f;

    CharacterController controller;
    Animator anim;

    float gravedad = -9.81f;
    float velocidadVertical = 0f;

    bool girando = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (anim != null)
            anim.SetFloat("speed", speed);

        InvokeRepeating(nameof(NuevaDireccion), tiempoParaNuevaDireccion, tiempoParaNuevaDireccion);
    }

    void Update()
    {
        AplicarMovimiento();
        DetectarObstaculo();
    }

    void AplicarMovimiento()
    {
        // Gravedad
        if (controller.isGrounded)
            velocidadVertical = -1f;
        else
            velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = transform.forward * speed;
        movimiento.y = velocidadVertical;

        controller.Move(movimiento * Time.deltaTime);
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
