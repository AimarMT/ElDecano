using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator animator;
    private bool opened = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        if (opened) return;

        Debug.Log("[DoorController] Opening door");
        animator.Play("Opening 1");
        opened = true;
    }
}
