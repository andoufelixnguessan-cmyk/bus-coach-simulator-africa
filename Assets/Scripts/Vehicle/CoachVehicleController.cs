using UnityEngine;

/// <summary>First-pass coach physics and live instrument data for the driving HUD.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class CoachVehicleController : MonoBehaviour
{
    [Header("Wheel colliders")]
    [SerializeField] private WheelCollider frontLeft, frontRight, rearLeft, rearRight;
    [Header("Vehicle tuning")]
    [SerializeField, Min(0f)] private float motorTorque = 4200f;
    [SerializeField, Min(0f)] private float serviceBrakeTorque = 8500f;
    [SerializeField, Min(0f)] private float handbrakeTorque = 12000f;
    [SerializeField, Range(5f, 45f)] private float maxSteerAngle = 28f;
    [SerializeField, Min(0f)] private float maxSpeedKph = 110f;
    [SerializeField] private float downforce = 80f;
    [Header("Center of mass")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.8f, 0.1f);

    private Rigidbody body;
    private float throttleInput, brakeInput, steeringInput;
    private bool handbrake;
    private float engineRpm = 650f;
    private float fuelPercent = 86f;
    private float coolantTemperatureC = 82f;
    private float oilPressureBar = 4.2f;
    private float airPressureBar = 8.2f;
    private int engineUpgradeLevel;

    public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
    public float EngineRpm => engineRpm;
    public float EngineRpmNormalized => Mathf.InverseLerp(600f, GetMaximumRpm(), engineRpm);
    public float FuelPercent => fuelPercent;
    public float CoolantTemperatureC => coolantTemperatureC;
    public float OilPressureBar => oilPressureBar;
    public float AirPressureBar => airPressureBar;
    public float BatteryVoltage => 24.2f;
    public bool EngineWarning => coolantTemperatureC >= 105f || oilPressureBar < 1.2f;
    public bool AbsWarning => false;
    public bool ParkingBrakeApplied => handbrake;
    public bool SeatbeltFastened { get; private set; } = true;
    public bool LeftIndicatorOn { get; private set; }
    public bool RightIndicatorOn { get; private set; }
    public bool HighBeamOn { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassOffset;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        float targetRpm = 650f + throttleInput * 1050f + Mathf.Clamp(SpeedKph * 12f, 0f, 1150f);
        if (SpeedKph < 1f && throttleInput < 0.05f) targetRpm = 650f;
        engineRpm = Mathf.Lerp(engineRpm, Mathf.Clamp(targetRpm, 600f, GetMaximumRpm()), Time.deltaTime * 2.4f);
        if (throttleInput > 0.2f && SpeedKph > 4f)
            fuelPercent = Mathf.Max(0f, fuelPercent - Time.deltaTime * (0.00018f + engineUpgradeLevel * 0.000015f));
        coolantTemperatureC = Mathf.Lerp(coolantTemperatureC, 82f + throttleInput * 15f, Time.deltaTime * 0.025f);
        oilPressureBar = Mathf.Lerp(oilPressureBar, 1.5f + engineRpm / 1000f, Time.deltaTime * 0.5f);
        airPressureBar = Mathf.Lerp(airPressureBar, handbrake ? 8.0f : 8.3f, Time.deltaTime * 0.4f);
    }

    public void SetInputs(float steering, float throttle, float brake, bool applyHandbrake = false)
    {
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        throttleInput = Mathf.Clamp01(throttle);
        brakeInput = Mathf.Clamp01(brake);
        handbrake = applyHandbrake;
    }

    public void Configure(WheelCollider fl, WheelCollider fr, WheelCollider rl, WheelCollider rr,
        float driveTorque, float brakeTorque, float steeringAngle, float speedLimitKph)
    {
        frontLeft = fl; frontRight = fr; rearLeft = rl; rearRight = rr;
        motorTorque = Mathf.Max(0f, driveTorque);
        serviceBrakeTorque = Mathf.Max(0f, brakeTorque);
        maxSteerAngle = Mathf.Clamp(steeringAngle, 5f, 45f);
        maxSpeedKph = Mathf.Max(10f, speedLimitKph);
    }

    /// <summary>Engine upgrades are applied by the garage and affect response, RPM ceiling and sound.</summary>
    public void ApplyEngineUpgrade(int level)
    {
        engineUpgradeLevel = Mathf.Clamp(level, 0, 5);
        motorTorque = 11000f * (1f + engineUpgradeLevel * 0.07f);
        maxSpeedKph = 105f + engineUpgradeLevel * 2f;
    }

    public void SetIndicators(bool left, bool right) { LeftIndicatorOn = left; RightIndicatorOn = right && !left; }
    public void SetHighBeam(bool enabled) { HighBeamOn = enabled; }
    public void SetSeatbelt(bool fastened) { SeatbeltFastened = fastened; }
    public float GetMaximumRpm() { return 2400f + engineUpgradeLevel * 100f; }

    private void FixedUpdate()
    {
        if (!WheelsAssigned()) return;
        float speedFactor = Mathf.InverseLerp(maxSpeedKph, 0f, SpeedKph);
        float torque = motorTorque * throttleInput * speedFactor;
        frontLeft.steerAngle = steeringInput * maxSteerAngle;
        frontRight.steerAngle = steeringInput * maxSteerAngle;
        rearLeft.motorTorque = torque * 0.5f;
        rearRight.motorTorque = torque * 0.5f;
        float brake = brakeInput * serviceBrakeTorque;
        frontLeft.brakeTorque = brake; frontRight.brakeTorque = brake;
        rearLeft.brakeTorque = brake + (handbrake ? handbrakeTorque : 0f);
        rearRight.brakeTorque = brake + (handbrake ? handbrakeTorque : 0f);
        body.AddForce(-transform.up * downforce * body.linearVelocity.magnitude);
    }

    private bool WheelsAssigned() => frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;
}
