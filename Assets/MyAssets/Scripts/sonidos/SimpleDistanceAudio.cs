using UnityEngine;

public class SimpleDistanceAudio : MonoBehaviour
{
    public Transform listener;     // Cámara XR
    public float maxDistance = 6f;

    private AudioSource audioSource;
    private float baseVolume;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        baseVolume = audioSource.volume;
    }

    void Start()
    {
        audioSource.Play(); // se reproduce una vez y nunca se para
    }

    void Update()
    {
        if (listener == null) return;

        float d = Vector3.Distance(transform.position, listener.position);

        float t = Mathf.Clamp01(1f - (d / maxDistance));
        audioSource.volume = baseVolume * t;
    }
}
