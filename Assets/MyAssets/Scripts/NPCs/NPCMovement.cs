using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    public float giroVelocidad = 200f;
    public float tiempoParaNuevaDireccion = 3f;

    private Vector3 direccion;

    void Start()
    {
        NuevaDireccion();
        InvokeRepeating(nameof(NuevaDireccion), tiempoParaNuevaDireccion, tiempoParaNuevaDireccion);
    }

    void Update()
    {
        transform.Translate(direccion * speed * Time.deltaTime, Space.World);
    }

    void NuevaDireccion()
    {
        direccion = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Si se choca con algo -> gira aleatoriamente
        transform.Rotate(Vector3.up * Random.Range(120f, 240f));
        NuevaDireccion();
    }
}
