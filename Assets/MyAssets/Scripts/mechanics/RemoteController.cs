using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RemoteController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject canvasMenu;

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
        // COMPROBACIÓN: ¿Es un socket (soporte) o es la mano?
        if (args.interactorObject is XRSocketInteractor)
        {
            // Si es un socket, no hacemos nada (la tele se queda apagada)
            Debug.Log("Mando colocado en el soporte. Tele apagada.");
            return;
        }

        // Si llegamos aquí, es que lo ha cogido el mando del jugador
        if (canvasMenu != null) canvasMenu.SetActive(true);
        Debug.Log("Mando agarrado por el jugador. Tele encendida.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Al soltarlo, siempre apagamos (ya sea que lo soltemos al aire o al soporte)
        if (canvasMenu != null) canvasMenu.SetActive(false);
    }
}