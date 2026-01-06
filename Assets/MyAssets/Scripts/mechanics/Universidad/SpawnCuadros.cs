using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnCuadros : MonoBehaviour
{
    public GameObject[] cuadros;

    private bool triggered = false;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.primerMinijuegoCompletado && !triggered)
            {
                triggered = true;
                foreach (GameObject cuadro in cuadros)
                {
                    if (cuadro != null)
                    {
                        cuadro.SetActive(true);
                    }
                }
            }
        }
    }
}
