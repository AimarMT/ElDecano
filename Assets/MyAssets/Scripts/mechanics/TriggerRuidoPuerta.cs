using UnityEngine;

public class TriggerRuidoPuerta : MonoBehaviour
{
    private bool triggered = false;
    public AudioSource audioSource;
    public AudioClip audioClip;




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            audioSource.PlayOneShot(audioClip);
            triggered = true;
        }
    }
}
