using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class ActivarPuzzle2 : MonoBehaviour
{
    [Header("Notas del puzzle (empiezan desactivadas)")]
    public GameObject[] notasPuzzle;

    [Header("Puntos posibles de aparición")]
    public Transform[] puntosSpawn;

    [Header("Audio")]
    public AudioClip audio1;

    private AudioSource audioSource;
    private bool puzzleIniciado = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Asegurar que el collider es trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.primerMinijuegoCompletado)
        {
            return;
        }
        if (puzzleIniciado)
            return;

        // Filtrar quién activa el trigger (ajusta tags si lo necesitas)
        if (!other.CompareTag("Player") && !other.CompareTag("XRHand"))
            return;

        puzzleIniciado = true;

        if (audio1 != null)
            audioSource.PlayOneShot(audio1);

        GameManager.Instance.triggerPiso1Activado = true;

        ColocarNotasAleatoriamente();

        Debug.Log("Nota principal activada por Trigger. Puzzle iniciado.");
    }

    private void ColocarNotasAleatoriamente()
    {
        if (puntosSpawn.Length < notasPuzzle.Length)
        {
            Debug.LogError("No hay suficientes puntos de spawn para las notas.");
            return;
        }

        List<Transform> puntosDisponibles = new List<Transform>(puntosSpawn);

        foreach (GameObject nota in notasPuzzle)
        {
            int index = Random.Range(0, puntosDisponibles.Count);
            Transform puntoElegido = puntosDisponibles[index];

            nota.transform.SetParent(null);

            Rigidbody rb = nota.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            nota.transform.position = puntoElegido.position;
            nota.transform.rotation = puntoElegido.rotation;

            nota.SetActive(true);
            puntosDisponibles.RemoveAt(index);
        }




        Debug.Log("Notas del puzzle colocadas aleatoriamente y listas.");
    }

}

