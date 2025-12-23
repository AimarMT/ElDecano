using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private Animator animator;
    private bool opened = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[DoorTrigger] Animator NOT found on this object");
        }
        else
        {
            Debug.Log("[DoorTrigger] Animator found successfully");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[DoorTrigger] OnTriggerEnter detected. Object: " + other.name);

        if (opened)
        {
            Debug.Log("[DoorTrigger] Door already opened. Ignoring trigger.");
            return;
        }

        Debug.Log("[DoorTrigger] Object tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[DoorTrigger] Player detected. Playing opening animation.");

            animator.Play("Opening 1");
            opened = true;
        }
        else
        {
            Debug.Log("[DoorTrigger] Object is not Player. Ignored.");
        }
    }
}
