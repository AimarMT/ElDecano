using UnityEngine;

public class CollisionAudioTriggerAndoni : MonoBehaviour
{
    [Header("Ajustes de Activación")]
    [Tooltip("Tiempo que el jugador debe estar dentro del área para que suene.")]
    public float stayTime = 0f; 
    
    private float timer = 0f;
    private bool isInside = false;
    private bool hasPlayed = false;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Nos aseguramos de que el Collider esté en modo Trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        // Si ya sonó o no hay nadie dentro, no hacemos nada
        if (hasPlayed || !isInside) return;

        timer += Time.deltaTime;

        if (timer >= stayTime)
        {
            PlayAudio();
        }
    }

    // Se activa cuando el Player entra en el Box Collider
    private void OnTriggerEnter(Collider other)
    {
        // Opcional: Verificar que el objeto que choca es el Player
        if (other.CompareTag("Player") || other.GetComponent<Camera>() != null)
        {
            isInside = true;
        }
    }

    // Se activa cuando el Player sale del Box Collider
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Camera>() != null)
        {
            isInside = false;
            timer = 0f; // Reiniciamos el tiempo si sale antes de que suene
        }
    }

    void PlayAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            hasPlayed = true;
            audioSource.Play();
            Debug.Log("Audio reproducido por colisión");
        }
    }
}