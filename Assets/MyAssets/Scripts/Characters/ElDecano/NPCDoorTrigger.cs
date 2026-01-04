using UnityEngine;
using SojaExiles;

public class NPCDoorTrigger : MonoBehaviour
{
    public opencloseDoor door1;
    public opencloseDoor1 door2;
    public string npcLayerName = "ElDecano";

    private void OnTriggerEnter(Collider other)
    {
        // 1. Diagnóstico: ¿Algo ha tocado el trigger?
        Debug.Log("Algo ha entrado en el trigger: " + other.name + " (Layer: " + LayerMask.LayerToName(other.gameObject.layer) + ")");

        if (other.gameObject.layer == LayerMask.NameToLayer(npcLayerName))
        {
            Debug.Log("¡EL DECANO DETECTADO! Intentando abrir...");

            if (door1 != null && !door1.open)
            {
                Debug.Log("Abriendo puerta 1");
                StartCoroutine(door1.opening());
            }

            if (door2 != null && !door2.open)
            {
                Debug.Log("Abriendo puerta 2");
                StartCoroutine(door2.opening());
            }
        }
        else 
        {
            Debug.Log("El objeto no está en el layer correcto. El layer detectado es: " + LayerMask.LayerToName(other.gameObject.layer));
        }
    }
}