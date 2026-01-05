using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(AudioSource))]
public class NotaPrincipalActivadorPuzzle : MonoBehaviour
{
    [Header("Notas del puzzle (empiezan desactivadas)")]
    public GameObject[] notasPuzzle;

    [Header("Puntos posibles de aparición")]
    public Transform[] puntosSpawn;

    [Header("Audio")]
    public AudioClip audioCogerNota;

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;

    private bool puzzleIniciado = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (puzzleIniciado)
            return;

        puzzleIniciado = true;

        // Audio al coger el papel
        if (audioCogerNota != null)
            audioSource.PlayOneShot(audioCogerNota);

        // Marcar nota como leída
        GameManager.Instance.notaUniLeida = true;

        // Activar y colocar notas del puzzle
        ColocarNotasAleatoriamente();

        Debug.Log(" Nota principal agarrada. Puzzle iniciado.");
    }

    private void ColocarNotasAleatoriamente()
    {
        if (puntosSpawn.Length < notasPuzzle.Length)
        {
            Debug.LogError(" No hay suficientes puntos de spawn para las notas.");
            return;
        }

        List<Transform> puntosDisponibles = new List<Transform>(puntosSpawn);

        foreach (GameObject nota in notasPuzzle)
        {
            int index = Random.Range(0, puntosDisponibles.Count);
            Transform puntoElegido = puntosDisponibles[index];

            // Desparentar para evitar rotaciones/escala extra
            nota.transform.SetParent(null);

            // Resetear Rigidbody para evitar saltos
            Rigidbody rb = nota.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false; // aseguramos que se pueda agarrar
            }

            // Colocar posición y rotación exacta
            nota.transform.position = puntoElegido.position;
            nota.transform.rotation = puntoElegido.rotation;

            // Activar la nota
            nota.SetActive(true);

            // Quitar el spawn usado
            puntosDisponibles.RemoveAt(index);
        }

        Debug.Log("📜 Notas del puzzle colocadas aleatoriamente y listas para XR.");
    }
}
