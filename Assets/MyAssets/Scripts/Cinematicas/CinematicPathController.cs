using UnityEngine;
using System.Collections;

public class CinematicPathController : MonoBehaviour
{
    [System.Serializable]
    public class WaypointAction
    {
        public Transform waypoint;
        public Transform groundOverride;
        public DoorController doorToOpen;
        public float waitTime;
    }

    public WaypointAction[] path;

    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float stopDistance = 0.1f;

    public float doorLookDuration = 0.35f;

    private int currentIndex = 0;
    private bool isPaused = false;

    private Animator animator;
    private GroundFollower groundFollower;

    void Start()
    {
        animator = GetComponent<Animator>();
        groundFollower = GetComponent<GroundFollower>();
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

        // 1) Si hay puerta: abrir y girar ANTES de SetForcedGround
        if (action.doorToOpen != null)
        {
            action.doorToOpen.OpenDoor();

            Vector3 lookPos = action.doorToOpen.transform.position;
            lookPos.y = transform.position.y;

            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation((lookPos - transform.position).normalized);

            float t = 0f;
            float duration = Mathf.Max(0.01f, doorLookDuration);

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            transform.rotation = targetRot;
        }

        // 2) Ahora sí: congelar suelo (y rotación) ya mirando a la puerta
        if (groundFollower != null)
        {
            groundFollower.SetForcedGround(action.groundOverride);
        }

        // 3) Espera
        if (action.waitTime > 0f)
        {
            yield return new WaitForSeconds(action.waitTime);
        }

        // 4) Soltar bloqueo
        if (groundFollower != null)
        {
            groundFollower.ClearForcedGround();
        }

        currentIndex++;
        isPaused = false;
    }
}
