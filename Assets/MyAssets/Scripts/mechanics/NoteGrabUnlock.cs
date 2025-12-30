using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NoteGrabUnlock : MonoBehaviour
{
    public Transform head;
    public float rotationSmooth = 10f;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Marcar la nota como leída
        GameManager.Instance.notaLeida = true;
        Debug.Log("Nota leída");
    }

    void Update()
    {
        if (grab.isSelected && head != null)
        {
            Vector3 direction = head.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);
        }
    }
}

