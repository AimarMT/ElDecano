using UnityEngine;

public class NPCInes : MonoBehaviour
{
    public Transform player;           // La cápsula o jugador
    public Transform playerCamera;     // La cámara dentro del jugador

    public Animator animator;          // Asignar desde el inspector

    public float timeToScream = 3f;   // Segundos que el jugador puede mirar
    public float distanceFromPlayer = 1f; // distancia frontal
    public float verticalOffset = 1.5f;   // altura para que la cara quede al nivel de la cámara

    private float lookTimer = 0f;
    private bool hasScreamed = false;

    void Update()
    {
        // 1. Inés mira al jugador todo el tiempo
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Opcional: solo gira en Y
        transform.rotation = Quaternion.LookRotation(direction);

        // 2. Detectar si el jugador la está mirando
        if (!hasScreamed && IsPlayerLookingAtNPC())
        {
            lookTimer += Time.deltaTime;

            if (lookTimer >= timeToScream)
            {
                TriggerScreamer();
            }
        }
        else if (!hasScreamed)
        {
            lookTimer = 0f; // Reinicia si deja de mirarla
        }
    }

    bool IsPlayerLookingAtNPC()
    {
        Vector3 toNPC = transform.position - playerCamera.position;
        toNPC.Normalize();

        float angle = Vector3.Angle(playerCamera.forward, toNPC);

        return angle < 30f; // Ajustable según cuán preciso quieras
    }

    void TriggerScreamer()
    {
        hasScreamed = true;

        // 1. Calcula el offset vertical desde el pivote hasta la cabeza
        float headHeightOffset = 2.35979986f - 0.843999982f; // Modelo.y - Bip.y
        float forwardOffset = -0.0116264112f; // Extract3.z

        // 2. Posiciona el NPC frente a la cámara
        Vector3 spawnPosition = playerCamera.position + playerCamera.forward * (distanceFromPlayer + forwardOffset);

        // Ajusta altura para que la cabeza quede al nivel de la cámara
        spawnPosition.y = playerCamera.position.y - headHeightOffset;

        transform.position = spawnPosition;

        // 3. Mira hacia la cámara
        Vector3 lookDirection = playerCamera.position - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        // 4. Activar animación de screamer
        if (animator != null)
        {
            animator.SetTrigger("Scream"); // Debes tener un Trigger llamado "Scream" en el Animator
        }

        Debug.Log("¡Screamer 3D activado con animación!");
    }
}
