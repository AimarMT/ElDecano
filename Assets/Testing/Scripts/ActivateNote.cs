using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ActivateNote : MonoBehaviour
{
    [Header("Input Action del botón B")]
    public InputActionReference activateButtonAction; // Acción de tipo Button en Input System

    private bool isHovering = false;
    private GameObject childCanvas;

    private void Awake()
    {
        // Obtenemos el Canvas hijo (puede estar desactivado)
        childCanvas = GetComponentInChildren<Canvas>(true)?.gameObject;
        if (childCanvas != null)
            childCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (activateButtonAction != null)
            activateButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        if (activateButtonAction != null)
            activateButtonAction.action.Disable();
    }

    public void OnHoverEnter()
    {
        isHovering = true;
    }

    public void OnHoverExit()
    {
        isHovering = false;
    }

    private void Update()
    {
        if (isHovering && childCanvas != null && activateButtonAction != null)
        {
            if (activateButtonAction.action.WasPressedThisFrame())
            {
                childCanvas.SetActive(true);
            }
        }
    }
}

