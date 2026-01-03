using UnityEngine;
using System.Collections;

public class CinematicWaypointEnd : MonoBehaviour
{
    [Header("Referencia al coche cinematico")]
    public CarCinnematicPath car;

    [Header("Indice del waypoint final")]
    public int endWaypointIndex = 2;

    [Header("Delay antes del fade")]
    public float delayBeforeFade = 1.5f;

    [Header("Controlador de fade")]
    public CinematicFadeScene fadeController;

    private bool triggered = false;

    void Update()
    {
        if (triggered) return;
        if (car == null) return;
        if (fadeController == null) return;

        if (car.CurrentWaypointIndex >= endWaypointIndex)
        {
            triggered = true;
            StartCoroutine(EndCinematic());
        }
    }

    private IEnumerator EndCinematic()
    {
        yield return new WaitForSeconds(delayBeforeFade);
        fadeController.StartFade();
    }
}
