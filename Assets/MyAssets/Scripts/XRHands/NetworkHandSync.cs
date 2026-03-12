using Unity.Netcode;
using UnityEngine;

public class NetworkHandSync : NetworkBehaviour
{
    [Header("Referencias de Red")]
    public Transform networkLeftHand;
    public Transform networkRightHand;

    // Referencias locales (las buscaremos al empezar)
    private Transform xrLeftHand;
    private Transform xrRightHand;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 1. Buscamos el XR Origin para seguirlo
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            xrLeftHand = xrOrigin.transform.Find("Camera Offset/Left Controller");
            xrRightHand = xrOrigin.transform.Find("Camera Offset/Right Controller");

            // 2. DESACTIVAR las manos del suelo para MÍ (pero no para otros)
            // Buscamos los SkinnedMeshRenderer en los hijos de red y los apagamos
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = false;
            }
        }
    }

    void Update()
    {
        // Muy importante: solo el "Owner" (tú) envía su posición a los demás
        if (IsOwner)
        {
            // Hacer que el avatar de red siga la posición de la cámara (tu cabeza)
            // Buscamos la cámara del XR Origin
            Transform cameraTransform = Camera.main.transform;

            transform.position = cameraTransform.position;

            // Opcional: Que el cuerpo gire hacia donde miras (solo en el eje Y)
            Vector3 newRotation = cameraTransform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, newRotation.y, 0);

            if (xrLeftHand != null)
            {
                networkLeftHand.position = xrLeftHand.position;
                networkLeftHand.rotation = xrLeftHand.rotation;
            }

            if (xrRightHand != null)
            {
                networkRightHand.position = xrRightHand.position;
                networkRightHand.rotation = xrRightHand.rotation;
            }
        }
    }
}