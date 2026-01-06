using System.Collections;
using UnityEngine;

public class SpawnDecanoPiso2 : MonoBehaviour
{
    [Header("Prefab a activar")]
    public GameObject prefabDecano; // ya hecho en escena, desactivado

    [Header("Audio (hasta 3 pistas)")]
    public AudioClip[] audioPistas;
    public AudioSource audioSource;

    [Header("Tiempo entre pistas")]
    public float delayEntrePistas = 0.75f;

    private bool triggered = false;

    private void Awake()
    {
        audioSource.playOnAwake = false;

        // Collider debe ser trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;  // solo una vez
        if (!other.CompareTag("Player")) return;
        if (!GameManager.Instance.notaUniLeida) return;
        
        triggered = true;
        
        // Activar prefab y moverlo a la posición del Empty

            prefabDecano.SetActive(true);


        // Reproducir pistas de audio en secuencia
        if (audioPistas != null && audioPistas.Length > 0)
        {
            StartCoroutine(PlayAudioSequence());
        }
    }

    private IEnumerator PlayAudioSequence()
    {
        foreach (AudioClip clip in audioPistas)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length + delayEntrePistas);
            }
        }
    }
}
