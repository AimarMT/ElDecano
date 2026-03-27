using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RemoteController2 : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject canvasMenu; // Arrastra aquí el GameObject MainMenu

    [Header("Ajustes de Agarre")]
    public Vector3 posicionAgarre = new Vector3(0, 0, 0);
    public Vector3 rotacionAgarre = new Vector3(0, 0, 0);

    private XRGrabInteractable grabInteractable;
    private GameObject customAttach;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        customAttach = new GameObject("PuntoDeAgarre_Auto");
        customAttach.transform.SetParent(this.transform);
        customAttach.transform.localPosition = posicionAgarre;
        customAttach.transform.localRotation = Quaternion.Euler(rotacionAgarre);
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
        if (args.interactorObject is XRSocketInteractor)
        {
            Debug.Log("Mando colocado en el soporte. Tele apagada.");
            return;
        }

        if (canvasMenu != null) canvasMenu.SetActive(true);
        Debug.Log("Mando agarrado por el jugador. Tele encendida.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (canvasMenu != null) canvasMenu.SetActive(false);
    }
}