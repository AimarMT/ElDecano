using UnityEngine;

public class TriggerSombraDecano: MonoBehaviour
{
    public MovimientoSombra sombra;
    
    public AudioSource audioSource;
    public AudioClip audioClip;
    public bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.notaLeida && !triggered)
        {
            sombra.StartShadowMovement();
            audioSource.PlayOneShot(audioClip);
            triggered = true;
        } else
        {
            return;
        }
    }
}
