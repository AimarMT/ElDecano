using UnityEngine;

public class StairsTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER STAIRS by " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT STAIRS by " + other.name);
    }
}
