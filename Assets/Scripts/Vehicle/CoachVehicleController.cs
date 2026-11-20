using UnityEngine;

/// <summary>
/// First-pass physics controller for a coach using WheelColliders.
/// Assign the wheel colliders and visual wheel transforms in the Inspector.
/// This is a foundation for tuning, not a finished vehicle simulation.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class CoachVehicleController : MonoBehaviour
{
    [Header("Wheel colliders")]
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider rearLeft;
    [SerializeField] private WheelCollider rearRight;

    [Header("Vehicle tuning")]
    [SerializeField, Min(0f)] private float motorTorque = 4200f;
    [SerializeField, Min(0f)] private float serviceBrakeTorque = 8500f;
    [SerializeField, Min(0f)] private float handbrakeTorque = 12000f;
    [SerializeField, Range(5f, 45f)] private float maxSteerAngle = 28f;
    [SerializeField, Min(0f)] private float maxSpeedKph = 110f;
    [SerializeField] private float downforce = 80f;

    [Header("Center of mass")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.8f, 0.1f);

    [Header("Wheel visuals (optional)")]
    [SerializeField] private Transform frontLeftVisual;
    [SerializeField] private Transform frontRightVisual;
    [SerializeField] private Transform rearLeftVisual;
    [SerializeField] private Transform rearRightVisual;

    private Rigidbody body;
    private float throttleInput;
    private float brakeInput;
    private float steeringInput;
    private bool handbrake;

    public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassOffset;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetInputs(float steering, float throttle, float brake, bool applyHandbrake = false)
    {
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        throttleInput = Mathf.Clamp01(throttle);
        brakeInput = Mathf.Clamp01(brake);
        handbrake = applyHandbrake;
    }

    private void FixedUpdate()
    {
        if (!WheelsAssigned()) return;

        float speedFactor = Mathf.InverseLerp(maxSpeedKph, 0f, SpeedKph);
        float torque = motorTorque * throttleInput * speedFactor;

        frontLeft.steerAngle = steeringInput * maxSteerAngle;
        frontRight.steerAngle = steeringInput * maxSteerAngle;

        // Rear-wheel drive, appropriate as a configurable starting point for a coach.
        rearLeft.motorTorque = torque * 0.5f;
        rearRight.motorTorque = torque * 0.5f;

        float brake = brakeInput * serviceBrakeTorque;
        frontLeft.brakeTorque = brake;
        frontRight.brakeTorque = brake;
        rearLeft.brakeTorque = brake + (handbrake ? handbrakeTorque : 0f);
        rearRight.brakeTorque = brake + (handbrake ? handbrakeTorque : 0f);

        body.AddForce(-transform.up * downforce * body.linearVelocity.magnitude);
        SyncWheel(frontLeft, frontLeftVisual);
        SyncWheel(frontRight, frontRightVisual);
        SyncWheel(rearLeft, rearLeftVisual);
        SyncWheel(rearRight, rearRightVisual);
    }

    private bool WheelsAssigned()
    {
        return frontLeft != null && frontRight != null &&
               rearLeft != null && rearRight != null;
    }

    private static void SyncWheel(WheelCollider collider, Transform visual)
    {
        if (collider == null || visual == null) return;
        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        visual.SetPositionAndRotation(position, rotation);
    }
}