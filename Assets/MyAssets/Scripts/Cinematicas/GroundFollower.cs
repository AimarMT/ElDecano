using UnityEngine;

public class GroundFollower : MonoBehaviour
{
    public float groundCheckDistance = 1.5f;
    public float heightOffset = 0.05f;
    public float smoothSpeed = 10f;

    public bool enabledFollower = true;

    void FixedUpdate()
    {
        if (!enabledFollower) return;

        RaycastHit hit;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            Vector3.down,
            out hit,
            groundCheckDistance
        ))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + heightOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.fixedDeltaTime
            );
        }
    }
}
