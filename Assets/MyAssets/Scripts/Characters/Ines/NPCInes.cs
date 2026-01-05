using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NPCInes : MonoBehaviour
{
    [Header("Referencias")]
    public Transform cameraTransform;

    [Header("Screamer Settings")]
    public float timeToScream = 3f;
    public float distanceFromPlayer = 0.8f; 
    public float rayDistance = 20f;
    [Tooltip("Ajusta la altura de Inés al aparecer (valores negativos la bajan)")]
    public float yOffsetSusto = -1.0f; // <--- NUEVA VARIABLE

    [Header("Audio")]
    [SerializeField] private AudioClip screamerAudio;
    [SerializeField] private float audioVolume = 1f;

    [Header("Scene Change")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float sceneLoadDelay = 1.2f;

    private float lookTimer;
    private bool hasTriggered;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
        audioSource.volume = audioVolume;

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (hasTriggered) return;

        MirarAlJugador();

        if (PlayerEstaMirando())
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= timeToScream) TriggerDeath();
        }
        else
        {
            lookTimer = 0f;
        }
    }

    void MirarAlJugador()
    {
        Vector3 dir = cameraTransform.position - transform.position;
        dir.y = 0f; 
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    bool PlayerEstaMirando()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, rayDistance))
        {
            if (hit.transform == transform) return true;
        }
        return false;
    }

    void TriggerDeath()
    {
        hasTriggered = true;

        if (screamerAudio != null) audioSource.PlayOneShot(screamerAudio);

        // 1. Posición: Justo frente a la cámara
        Vector3 targetPos = cameraTransform.position + (cameraTransform.forward * distanceFromPlayer);
        
        // 2. Altura: Usamos la cámara como base y sumamos el offset que elijas en el inspector
        targetPos.y = cameraTransform.position.y + yOffsetSusto; 
        transform.position = targetPos;

        // 3. Rotación: Forzamos que se mantenga vertical
        Vector3 lookDir = cameraTransform.position - transform.position;
        lookDir.y = 0; 
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}