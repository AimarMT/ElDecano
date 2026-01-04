using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NPCInes : MonoBehaviour
{
    public Transform player;
    public Transform playerCamera;

    [Header("Screamer Settings")]
    public float timeToScream = 3f;
    public float distanceFromPlayer = 1f;

    [Header("Manual Position")]
    [SerializeField] private float spawnHeight = 1.8f;
    [SerializeField] private float forwardOffset = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip screamerAudio;
    [SerializeField] private float audioVolume = 1f;

    [Header("Scene Change")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float sceneLoadDelay = 0.5f; // ⏱ tiempo para oír el audio

    private float lookTimer;
    private bool hasTriggered;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.volume = audioVolume;
    }

    void Update()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (!hasTriggered && IsPlayerLookingAtNPC())
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= timeToScream)
                TriggerDeath();
        }
        else if (!hasTriggered)
        {
            lookTimer = 0f;
        }
    }

    bool IsPlayerLookingAtNPC()
    {
        Vector3 toNPC = transform.position - playerCamera.position;
        toNPC.Normalize();
        return Vector3.Angle(playerCamera.forward, toNPC) < 30f;
    }

    void TriggerDeath()
    {
        hasTriggered = true;

        // 🔊 Audio
        if (screamerAudio != null)
        {
            audioSource.clip = screamerAudio;
            audioSource.Play();
        }

        // Posicionar NPC frente al jugador
        Vector3 spawnPosition =
            playerCamera.position +
            playerCamera.forward * (distanceFromPlayer + forwardOffset);

        spawnPosition.y = spawnHeight;
        transform.position = spawnPosition;

        // Rotar solo en Y
        Vector3 lookDirection = playerCamera.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection);

        // ⏱ Esperar antes de cambiar de escena
        StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}
