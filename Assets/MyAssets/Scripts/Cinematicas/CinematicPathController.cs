using UnityEngine;
using System.Collections;

public class CinematicPathController : MonoBehaviour
{
    [System.Serializable]
    public class WaypointAction
    {
        public Transform waypoint;
        public Transform groundOverride;   // Suelo correcto cuando se para aquí
        public DoorController doorToOpen;  // Opcional
        public float waitTime;              // 0 si no hay espera
    }

    public WaypointAction[] path;

    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float stopDistance = 0.1f;

    private int currentIndex = 0;
    private bool isPaused = false;

    private Animator animator;
    private GroundFollower groundFollower;

    void Start()
    {
        animator = GetComponent<Animator>();
        groundFollower = GetComponent<GroundFollower>();

        if (path == null || path.Length == 0)
        {
            Debug.LogError("[CinematicPathController] No path assigned");
        }
    }

    void Update()
    {
        if (isPaused || currentIndex >= path.Length)
        {
            animator.SetFloat("speed", 0f);
            return;
        }

        Transform target = path[currentIndex].waypoint;

        Vector3 targetPos = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= stopDistance)
        {
            animator.SetFloat("speed", 0f);
            StartCoroutine(HandleWaypoint());
            return;
        }

        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        animator.SetFloat("speed", moveSpeed);
    }

    private IEnumerator HandleWaypoint()
    {
        isPaused = true;

        WaypointAction action = path[currentIndex];

        if (groundFollower != null)
        {
            groundFollower.SetForcedGround(action.groundOverride);
        }

        if (action.doorToOpen != null)
        {
            action.doorToOpen.OpenDoor();
        }

        if (action.waitTime > 0f)
        {
            yield return new WaitForSeconds(action.waitTime);
        }

        if (groundFollower != null)
        {
            groundFollower.ClearForcedGround();
        }

        currentIndex++;
        isPaused = false;
    }
}
