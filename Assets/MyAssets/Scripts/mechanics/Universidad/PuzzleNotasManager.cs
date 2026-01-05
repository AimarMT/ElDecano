using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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

        // Si llega aquí, TODO es correcto
        PuzzleCompletado();
    }

    private void PuzzleCompletado()
    {
        puzzleCompletado = true;

        GameManager.Instance.primerMinijuegoCompletado = true;

        foreach (var s in sockets)
        {
            // Desactivar el socket
            s.socket.enabled = false;

            // Bloquear la nota
            GameObject nota = s.socket.firstInteractableSelected.transform.gameObject;

  
            
        }

        Debug.Log(" Minijuego de notas completado");
    }
}

