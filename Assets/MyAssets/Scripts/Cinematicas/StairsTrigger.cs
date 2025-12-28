using UnityEngine;

public class StairsTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Animator animator = other.GetComponentInParent<Animator>();
        if (animator == null) return;

        animator.SetBool("stairs", true);
        Debug.Log("[StairsTrigger] Enter stairs -> stairs = true");
    }

    private void OnTriggerExit(Collider other)
    {
        Animator animator = other.GetComponentInParent<Animator>();
        if (animator == null) return;

        animator.SetBool("stairs", false);
        Debug.Log("[StairsTrigger] Exit stairs -> stairs = false");
    }
}
