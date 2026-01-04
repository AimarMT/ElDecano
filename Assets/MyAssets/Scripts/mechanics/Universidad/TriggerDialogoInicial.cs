using System.Collections;
using UnityEngine;

public class TriggerDialogoInicial : MonoBehaviour
{
    public bool triggered = false;
    public float tiempoPrimerAudio= 2f;
    public float tiempoSegundoAudio = 2f;


    public AudioSource audioSource;

    public AudioClip audioClip1;
    public AudioClip audioClip2;
    public AudioClip audioClip3;


    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(PistasAudio(tiempoPrimerAudio, tiempoSegundoAudio));

        }
    }

    IEnumerator PistasAudio(float tiempo1, float tiempo2)
    {
        audioSource.PlayOneShot(audioClip1);
        
        yield return new WaitForSeconds(tiempo1);
        audioSource.PlayOneShot(audioClip2);
        yield return new WaitForSeconds(tiempo2);
        audioSource.PlayOneShot(audioClip3);
    }
}
