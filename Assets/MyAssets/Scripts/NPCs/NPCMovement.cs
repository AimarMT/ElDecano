using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    public float giroVelocidad = 200f;
    public float tiempoParaNuevaDireccion = 3f;

    void Start()
    {
        GetComponent<Animator>().SetFloat("speed", 2);
        InvokeRepeating(nameof(NuevaDireccion), tiempoParaNuevaDireccion, tiempoParaNuevaDireccion);
    }

    void Update()
    {
        // Mover SIEMPRE hacia adelante
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void NuevaDireccion()
    {
        // Solo giramos, sin cambiar la dirección de movimiento
        float giro = Random.Range(-180f, 180f);
        transform.Rotate(0, giro, 0);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Si se choca, gira un poco y sigue hacia delante
        float giroAleatorio = Random.Range(120f, 240f);
        transform.Rotate(0, giroAleatorio, 0);
    }
}
