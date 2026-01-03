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

    [Header("Velocidad")]
    public float minSpeed = 4f;
    public float maxSpeed = 20f;
    public float acceleration = 12f;

    [Header("Movimiento")]
    public float rotationSpeed = 5f;
    public float waypointTolerance = 0.4f;

    [Header("Suavizado de trayectoria")]
    public float steeringSmoothTime = 0.35f;

    [Header("Ruedas delanteras (direccion visual)")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;

    [Range(0f, 5f)]
    public float maxWheelSteerAngle = 2.5f;
    public float wheelTurnSpeed = 8f;

    private int currentIndex = 0;
    private float currentSpeed = 0f;
    private float currentWheelAngle = 0f;

    private CarMoveDirection currentDirection;

    private Vector3 smoothSteerDir;
    private Vector3 steerVelocity;

    private Quaternion flBaseRot;
    private Quaternion frBaseRot;

    private float debugTimer = 0f;

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentDirection = waypoints[0].direction;
        currentSpeed = minSpeed;

        smoothSteerDir = transform.forward;
        steerVelocity = Vector3.zero;

        if (frontLeftWheel)
            flBaseRot = frontLeftWheel.localRotation;

        if (frontRightWheel)
            frBaseRot = frontRightWheel.localRotation;
    }

    void Update()
    {
        if (currentIndex >= waypoints.Length) return;

        CarWaypoint current = waypoints[currentIndex];
        Transform wp = current.waypoint;

        Vector3 carPos = transform.position;
        Vector3 toTarget = wp.position - carPos;
        float distance = toTarget.magnitude;

        if (current.direction != currentDirection)
        {
            currentSpeed = minSpeed;
            currentDirection = current.direction;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            maxSpeed,
            acceleration * Time.deltaTime
        );

        // ROTACION
        Vector3 desiredLookDir =
            current.direction == CarMoveDirection.Forward
                ? toTarget.normalized
                : -toTarget.normalized;

        smoothSteerDir = Vector3.SmoothDamp(
            smoothSteerDir,
            desiredLookDir,
            ref steerVelocity,
            steeringSmoothTime
        );

        if (smoothSteerDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(smoothSteerDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // MOVIMIENTO: SIEMPRE HACIA EL WAYPOINT
        Vector3 moveDir = toTarget.normalized;
        transform.position += moveDir * currentSpeed * Time.deltaTime;

        // RUEDAS
        float targetWheelAngle = 0f;

        if (distance > 0.01f)
        {
            Vector3 localTarget = transform.InverseTransformPoint(wp.position);
            float turn = Mathf.Clamp(localTarget.x, -1f, 1f);
            targetWheelAngle = turn * maxWheelSteerAngle;
        }

        currentWheelAngle = Mathf.Lerp(
            currentWheelAngle,
            targetWheelAngle,
            wheelTurnSpeed * Time.deltaTime
        );

        if (frontLeftWheel)
            frontLeftWheel.localRotation =
                flBaseRot * Quaternion.Euler(0f, currentWheelAngle, 0f);

        if (frontRightWheel)
            frontRightWheel.localRotation =
                frBaseRot * Quaternion.Euler(0f, currentWheelAngle, 0f);

        if (distance < waypointTolerance)
        {
            currentIndex++;
            steerVelocity = Vector3.zero;
            smoothSteerDir = transform.forward;
        }

        debugTimer += Time.deltaTime;
        if (debugTimer >= 1f)
        {
            debugTimer = 0f;
            Debug.Log(
                "CAR POS: " + carPos.ToString("F2") +
                " | WB INDEX: " + currentIndex +
                " | WB POS: " + wp.position.ToString("F2") +
                " | TO WB: " + toTarget.ToString("F2") +
                " | DIST: " + distance.ToString("F2") +
                " | DIR: " + current.direction
            );
        }
    }
    public int CurrentWaypointIndex
    {
        get { return currentIndex; }
    }
}
