using UnityEngine;

public enum CarMoveDirection
{
    Forward,
    Reverse
}

[System.Serializable]
public class CarWaypoint
{
    public Transform waypoint;
    public CarMoveDirection direction;
}

public class CarCinnematicPath : MonoBehaviour
{
    [Header("Waypoints (orden manual)")]
    public CarWaypoint[] waypoints;

    [Header("Movimiento del coche")]
    public float speed = 6f;
    public float rotationSpeed = 5f;
    public float waypointTolerance = 0.4f;

    [Header("Ruedas delanteras")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public float maxSteerAngle = 30f;
    public float wheelTurnSpeed = 8f;

    private int currentIndex = 0;
    private float currentSteerAngle = 0f;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentIndex >= waypoints.Length) return;

        CarWaypoint current = waypoints[currentIndex];
        Transform wp = current.waypoint;

        Vector3 toTarget = wp.position - transform.position;

        // =========================
        // MOVIMIENTO DEL COCHE
        // =========================
        Vector3 moveDir =
            current.direction == CarMoveDirection.Forward
                ? transform.forward
                : -transform.forward;

        transform.position += moveDir * speed * Time.deltaTime;

        // =========================
        // ROTACIÓN DEL COCHE (solo forward)
        // =========================
        if (current.direction == CarMoveDirection.Forward && toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // =========================
        // GIRO DE RUEDAS DELANTERAS
        // =========================
        float steerTarget = 0f;

        if (toTarget.sqrMagnitude > 0.01f)
        {
            Vector3 localTarget = transform.InverseTransformPoint(wp.position);
            float turnDir = Mathf.Clamp(localTarget.x, -1f, 1f);
            steerTarget = turnDir * maxSteerAngle;
        }

        currentSteerAngle = Mathf.Lerp(
            currentSteerAngle,
            steerTarget,
            wheelTurnSpeed * Time.deltaTime
        );

        if (frontLeftWheel && frontRightWheel)
        {
            frontLeftWheel.localRotation =
                Quaternion.Euler(0f, currentSteerAngle, 0f);

            frontRightWheel.localRotation =
                Quaternion.Euler(0f, currentSteerAngle, 0f);
        }

        // =========================
        // LLEGADA AL WAYPOINT
        // =========================
        if (Vector3.Distance(transform.position, wp.position) < waypointTolerance)
        {
            currentIndex++;
        }
    }
}
