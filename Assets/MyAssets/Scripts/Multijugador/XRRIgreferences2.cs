using UnityEngine;

public class XRRigReferences2 : MonoBehaviour
{
    public static XRRigReferences2 Instance;

    public Transform root;         // El XR Origin
    public Transform cameraOffset; // EL NUEVO: Arrastra aquí el objeto "Camera Offset"
    public Transform head;         // La Main Camera
    public Transform leftHand;
    public Transform rightHand;

    private void Awake()
    {
        Instance = this;
    }
}