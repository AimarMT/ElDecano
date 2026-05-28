using UnityEngine;

public class XRRigReferences2 : MonoBehaviour
{
    // Ya NO usamos singleton estático porque en Multiplayer Play Mode
    // host y cliente comparten el mismo proceso y se sobreescriben mutuamente.
    // Cada NetworkPlayer2 busca su propio rig por tag en su OnNetworkSpawn.
    public Transform root;
    public Transform cameraOffset;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
}