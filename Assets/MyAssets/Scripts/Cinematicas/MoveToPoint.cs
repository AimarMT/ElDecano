using UnityEngine;

public class MoveToPoint : MonoBehaviour
{
    [Header("Waypoint")]
    public Transform targetPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float stopDistance = 0.1f;

    private Animator animator;
    private bool wasMoving = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[MoveToPoint] Animator NOT found on player");
        }
        else
        {
            Debug.Log("[MoveToPoint] Animator found successfully");
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("[MoveToPoint] TargetPoint is NOT assigned");
        }
        else
        {
            Debug.Log("[MoveToPoint] TargetPoint assigned: " + targetPoint.name);
        }
    }

    void Update()
    {
        if (targetPoint == null)
        {
            animator.SetFloat("speed", 0f);
            return;
        }

        Vector3 direction = targetPoint.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            if (!wasMoving)
            {
                Debug.Log("[MoveToPoint] Starting to move towards target");
                wasMoving = true;
            }

            animator.SetFloat("speed", moveSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            if (wasMoving)
            {
                Debug.Log("[MoveToPoint] Target reached. Stopping movement");
                wasMoving = false;
            }

            animator.SetFloat("speed", 0f);
        }
    }
}
