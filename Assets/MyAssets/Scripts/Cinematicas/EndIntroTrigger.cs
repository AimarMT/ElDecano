using UnityEngine;

public class EndIntroTrigger : MonoBehaviour
{
    public FadeAndLoadScene fadeController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fadeController.StartFade();
            gameObject.SetActive(false);
        }
    }
}
