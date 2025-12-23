using UnityEngine;
using System.Collections;

public class CinematicPathController : MonoBehaviour
{
    [System.Serializable]
    public class WaypointAction
    {
        public Transform waypoint;
        public DoorController doorToOpen; // opcional
        public float waitTime;             // 0 si no hay espera
    }

    public WaypointAction[] path;

    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float stopDistance = 0.1f;

    private int currentIndex = 0;
    private bool isPaused = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

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

        // 1. COMPROBAR LLEGADA PRIMERO
        if (distance <= stopDistance)
        {
            animator.SetFloat("speed", 0f);
            Debug.Log("[CinematicPathController] Reached " + target.name);
            StartCoroutine(HandleWaypoint());
            return;
        }

        // 2. ROTAR (solo visual)
        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // 3. MOVER DE FORMA SEGURA
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

        // Abrir puerta si existe
        if (action.doorToOpen != null)
        {
            Debug.Log("[CinematicPathController] Opening door");
            action.doorToOpen.OpenDoor();
        }

        // Esperar si hace falta
        if (action.waitTime > 0f)
        {
            yield return new WaitForSeconds(action.waitTime);
        }

        currentIndex++;
        isPaused = false;
    }
}
