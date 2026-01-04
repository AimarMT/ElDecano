using UnityEngine;

public class VRCinematicFollower : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform cinematicCamera;

    void LateUpdate()
    {
        if (xrOrigin == null || cinematicCamera == null) return;

        xrOrigin.position = cinematicCamera.position;
        xrOrigin.rotation = cinematicCamera.rotation;
    }
}
