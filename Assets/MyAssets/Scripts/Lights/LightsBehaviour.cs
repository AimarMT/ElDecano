using UnityEngine;

public class LightsBehaviour : MonoBehaviour
{
    public Light luz;
    public float baseIntensity = 1f;   
    public float flickerAmount = 0.5f; 

    void Start()
    {
        if (luz == null)
            luz = GetComponent<Light>();
    }

    void Update()
    {
        luz.intensity = baseIntensity + Random.Range(-flickerAmount, flickerAmount);
    }
}
