using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PuzzleNotasManager : MonoBehaviour
{
    [System.Serializable]
    public class SocketNotaCorrecta
    {
        public XRSocketInteractor socket;
        public string tagCorrecto;
    }

    [Header("Configuración del puzzle")]
    public List<SocketNotaCorrecta> sockets;

    public AudioSource audioSource;
    public AudioClip audioClip1;
    public AudioClip audioClip2;

    private bool puzzleCompletado = false;

    void OnEnable()
    {
        foreach (var s in sockets)
        {
            s.socket.selectEntered.AddListener(OnNotaColocada);
            s.socket.selectExited.AddListener(OnNotaQuitada);
        }
    }

    void OnDisable()
    {
        foreach (var s in sockets)
        {
            s.socket.selectEntered.RemoveListener(OnNotaColocada);
            s.socket.selectExited.RemoveListener(OnNotaQuitada);
        }
    }

    private void OnNotaColocada(SelectEnterEventArgs args)
    {
        ComprobarPuzzle();
    }

    private void OnNotaQuitada(SelectExitEventArgs args)
    {
        if (!puzzleCompletado)
            return;

        // Si ya estaba completado, no permitimos quitar nada
        args.interactorObject.transform.SetParent(args.interactableObject.transform);
    }

    private void ComprobarPuzzle()
    {
        foreach (var s in sockets)
        {
            if (!s.socket.hasSelection)
                return;

            GameObject nota = s.socket.firstInteractableSelected.transform.gameObject;

            if (!nota.CompareTag(s.tagCorrecto))
                return;
        }

        // Si llega aquí, todas las notas están correctas
        PuzzleCompletado();
    }

    private void PuzzleCompletado()
    {
        puzzleCompletado = true;

        // Actualizar el estado de minijuegos
        if (!GameManager.Instance.primerMinijuegoCompletado)
        {
            GameManager.Instance.primerMinijuegoCompletado = true;
        }
        else
        {
            GameManager.Instance.segundoMinijuegoCompletado = true;
        }

        // Bloquear todas las notas y desactivar sockets
        foreach (var s in sockets)
        {
            if (s.socket.hasSelection)
            {
                GameObject nota = s.socket.firstInteractableSelected.transform.gameObject;

                // Romper relación con el socket
                nota.transform.SetParent(null);

                // Desactivar XRGrabInteractable si existe
                XRGrabInteractable grab = nota.GetComponent<XRGrabInteractable>();
                if (grab != null)
                    grab.enabled = false;

                // Hacer Rigidbody cinemático y desactivar gravedad
                Rigidbody rb = nota.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }

            // Desactivar el socket
            s.socket.enabled = false;
        }
        StartCoroutine(PlayAudios());
        Debug.Log("Minijuego de notas completado");
    }

     IEnumerator PlayAudios()
    {
        audioSource.PlayOneShot(audioClip1);
        yield return new WaitForSeconds(0.5f);
        audioSource.PlayOneShot(audioClip2);

    }

}
