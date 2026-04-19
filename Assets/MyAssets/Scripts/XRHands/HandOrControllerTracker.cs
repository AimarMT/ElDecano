using UnityEngine;

public class HandOrControllerTracker : MonoBehaviour
{
    [Header("Mando")]
    [SerializeField] private Transform controllerTransform;

    [Header("Mano física")]
    [SerializeField] private Transform handTransform;

    void Update()
    {
        if (handTransform != null &&
            handTransform.gameObject.activeInHierarchy)
        {
            // Mano física activa
            transform.SetPositionAndRotation(
                handTransform.position,
                handTransform.rotation);
        }
        else
        {
            // Mando activo
            if (controllerTransform != null)
                transform.SetPositionAndRotation(
                    controllerTransform.position,
                    controllerTransform.rotation);
        }
    }
}