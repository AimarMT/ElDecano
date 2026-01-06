using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
   
    [HideInInspector] public bool notaLeida = false;
    [HideInInspector] public bool notaUniLeida = false;
    [HideInInspector] public bool triggerPiso1Activado = false;
    [HideInInspector] public bool irSegundoPiso = false;
    [HideInInspector] public bool primerMinijuegoCompletado = false;

    [HideInInspector] public bool segundoMinijuegoCompletado = false;
    

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}

