using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RemoteController : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("Arrastra aquí el Canvas que quieres que aparezca")]
    public GameObject canvasMenu;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        // Obtenemos la referencia al componente de agarre
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("¡Falta el componente XRGrabInteractable en este objeto!");
        }
    }

    void Start()
    {
        // Nos aseguramos de que el menú empiece oculto
        if (canvasMenu != null)
        {
            canvasMenu.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos de agarre
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        // Desvincularse para evitar errores de memoria
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (canvasMenu != null)
        {
            canvasMenu.SetActive(true);
            Debug.Log("Mando agarrado: Menú activado");
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (canvasMenu != null)
        {
            canvasMenu.SetActive(false);
            Debug.Log("Mando soltado: Menú desactivado");
        }
    }
}