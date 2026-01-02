using UnityEngine;

public class GroundFollower : MonoBehaviour
{
    public float groundCheckDistance = 2f;
    public float heightOffset = 0.05f;
    public float smoothSpeed = 10f;

    public bool enabledFollower = true;

    private Transform forcedGround;
    private bool useForcedGround = false;

    private Quaternion lockedRotation;
    private float lockedY;

    private bool rotationLocked = false;

    public void SetForcedGround(Transform ground)
    {
        forcedGround = ground;
        useForcedGround = ground != null;

        if (useForcedGround)
        {
            lockedRotation = transform.rotation;
            lockedY = transform.position.y;
            rotationLocked = true;
        }
    }

    public void ClearForcedGround()
    {
        forcedGround = null;
        useForcedGround = false;
        rotationLocked = false;
    }

    void LateUpdate()
    {
        if (!enabledFollower) return;

        if (useForcedGround)
        {
            Vector3 pos = transform.position;
            pos.y = lockedY;
            transform.position = pos;

            if (rotationLocked)
            {
                transform.rotation = lockedRotation;
            }

            return;
        }

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + heightOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}
