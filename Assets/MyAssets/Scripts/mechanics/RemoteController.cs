using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RemoteController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject canvasMenu;

    [Header("Ajustes de Agarre (Dónde aparece la mano)")]
    public Vector3 posicionAgarre = new Vector3(0, 0, 0);
    public Vector3 rotacionAgarre = new Vector3(0, 0, 0);

    private XRGrabInteractable grabInteractable;
    private GameObject customAttach;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // CREACIÓN AUTOMÁTICA DEL PUNTO DE ANCLAJE
        // Creamos un objeto hijo que servirá como "imán" para la mano
        customAttach = new GameObject("PuntoDeAgarre_Auto");
        customAttach.transform.SetParent(this.transform);
        
        // Aplicamos tus ajustes del inspector
        customAttach.transform.localPosition = posicionAgarre;
        customAttach.transform.localRotation = Quaternion.Euler(rotacionAgarre);

        // Se lo asignamos al componente de Unity
        grabInteractable.attachTransform = customAttach.transform;
    }

    void Start()
    {
        if (canvasMenu != null) canvasMenu.SetActive(false);
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (canvasMenu != null) canvasMenu.SetActive(true);
        Debug.Log("Mando agarrado por el punto de anclaje configurado.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (canvasMenu != null) canvasMenu.SetActive(false);
    }
}