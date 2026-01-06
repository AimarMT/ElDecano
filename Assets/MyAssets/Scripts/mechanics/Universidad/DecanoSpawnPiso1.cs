using System.Collections;
using UnityEngine;

public class DecanoSpawnPiso1 : MonoBehaviour
{
    [Header("Prefab a activar")]
    public GameObject prefabDecano; 




    private bool triggered = false;

    private void Awake()
    {

        // Collider debe ser trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;  // solo una vez
        if (!other.CompareTag("Player")) return;
        if (!GameManager.Instance.primerMinijuegoCompletado) return;

        triggered = true;


        prefabDecano.SetActive(true);


    }


}
