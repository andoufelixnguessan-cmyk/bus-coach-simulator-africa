using UnityEngine;

/// <summary>First-pass coach physics and live instrument data for the driving HUD.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class CoachVehicleController : MonoBehaviour
{
    public enum GearSelection { Park, Reverse, Neutral, Drive }
    private static readonly float[] UpshiftSpeeds = { 18f, 32f, 48f, 66f, 86f };
    private static readonly float[] DownshiftSpeeds = { 12f, 24f, 38f, 54f, 74f };
    private static readonly float[] GearTorqueMultipliers = { 0f, 1.65f, 1.35f, 1.12f, 0.95f, 0.82f, 0.72f };
    private static readonly float[] RpmPerKphByGear = { 0f, 55f, 36f, 27f, 21f, 17f, 14f };
    [Header("Wheel colliders")]
    [SerializeField] private WheelCollider frontLeft, frontRight, rearLeft, rearRight, rearLeftInner, rearRightInner;
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
    private GearSelection gearSelection = GearSelection.Park;
    private int automaticGear = 1;
    private float gearShiftCooldown;
    private bool absActive;
    private float accessoryLoad;
    private Light leftHeadlight, rightHeadlight;
    private Light[] indicatorLights = System.Array.Empty<Light>();
    private Light[] brakeLights = System.Array.Empty<Light>();
    private Light[] tailLights = System.Array.Empty<Light>();
    private Light[] reverseLights = System.Array.Empty<Light>();
    private float leftHeadlightBaseIntensity = 2.2f;
    public bool HeadlightsOn { get; private set; }
    public bool HazardLightsOn { get; private set; }

    public bool EngineRunning { get; private set; }
    public float SteeringInput => steeringInput;
    public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
    public float EngineRpm => engineRpm;
    public float EngineRpmNormalized => Mathf.InverseLerp(600f, GetMaximumRpm(), engineRpm);
    public float FuelPercent => fuelPercent;
    public float AccessoryLoad => accessoryLoad;
    public float CoolantTemperatureC => coolantTemperatureC;
    public float OilPressureBar => oilPressureBar;
    public float AirPressureBar => airPressureBar;
    public bool AirPressureWarning => airPressureBar < 5.5f;
    public bool FuelWarning => fuelPercent < 15f;
    public bool DoorWarning => PassengerDoorOpen;
    public float BatteryVoltage => 24.2f;
    public bool EngineWarning => EngineRunning && (coolantTemperatureC >= 105f || oilPressureBar < 1.2f);
    public bool AbsWarning => absActive;
    public bool ParkingBrakeApplied => handbrake || gearSelection == GearSelection.Park || airPressureBar < 1f;
    public GearSelection SelectedGear => gearSelection;
    public int AutomaticGear => automaticGear;
    public string CurrentGearLabel => gearSelection == GearSelection.Drive ? "D" + automaticGear : gearSelection == GearSelection.Reverse ? "R" : gearSelection == GearSelection.Neutral ? "N" : "P";
    public bool SeatbeltFastened { get; private set; } = true;
    public bool PassengerDoorOpen { get; private set; }
    public bool LeftIndicatorOn { get; private set; }
    public bool RightIndicatorOn { get; private set; }
    public bool HighBeamOn { get; private set; }
    public bool IndicatorsBlinking => HazardLightsOn || LeftIndicatorOn || RightIndicatorOn;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass += centerOfMassOffset;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        UpdateAutomaticGearbox();
        float rpmFactor = gearSelection == GearSelection.Drive
            ? RpmPerKphByGear[Mathf.Clamp(automaticGear, 1, 6)]
            : gearSelection == GearSelection.Reverse ? 30f : 0f;
        float targetRpm = EngineRunning
            ? 650f + Mathf.Clamp(SpeedKph * rpmFactor, 0f, 1500f) + throttleInput * 520f
            : 0f;
        if (EngineRunning && SpeedKph < 1f && throttleInput < 0.05f) targetRpm = 650f;
        engineRpm = Mathf.Lerp(engineRpm, EngineRunning ? Mathf.Clamp(targetRpm, 600f, GetMaximumRpm()) : 0f, Time.deltaTime * 2.4f);
        if (EngineRunning)
        {
            float fuelRate = 0.00012f + throttleInput * 0.0013f +
                SpeedKph * 0.000018f + accessoryLoad * 0.00045f +
                engineUpgradeLevel * 0.00001f;
            fuelPercent = Mathf.Max(0f, fuelPercent - Time.deltaTime * fuelRate);
        }
        float targetTemp = EngineRunning ? 84f + throttleInput * 22f - Mathf.Clamp(SpeedKph * 0.04f, 0f, 5f) : 25f;
        float coolingRate = EngineRunning ? 0.018f + Mathf.Clamp(SpeedKph * 0.00025f, 0f, 0.025f) : 0.008f;
        coolantTemperatureC = Mathf.Lerp(coolantTemperatureC, targetTemp, Time.deltaTime * coolingRate);
        oilPressureBar = Mathf.Lerp(oilPressureBar, EngineRunning ? 1.5f + engineRpm / 1000f : 0f, Time.deltaTime * 0.5f);
        float airSupply = EngineRunning ? 0.08f : 0f;
        float airUse = brakeInput * 0.18f + (handbrake ? 0.025f : 0f);
        airPressureBar = Mathf.Clamp(airPressureBar + (airSupply - airUse) * Time.deltaTime, 0f, 8.3f);
        UpdateExteriorIndicators();
        UpdateBrakeLights();
        UpdateReverseLights();
    }

    private void UpdateAutomaticGearbox()
    {
        if (gearShiftCooldown > 0f) gearShiftCooldown -= Time.deltaTime;
        if (!EngineRunning || gearSelection != GearSelection.Drive || gearShiftCooldown > 0f) return;

        int index = Mathf.Clamp(automaticGear - 1, 0, UpshiftSpeeds.Length - 1);
        if (automaticGear < 6 && SpeedKph >= UpshiftSpeeds[index] && engineRpm > 1850f)
        {
            automaticGear++;
            gearShiftCooldown = 0.65f;
        }
        else if (automaticGear > 1 && SpeedKph <= DownshiftSpeeds[automaticGear - 2])
        {
            automaticGear--;
            gearShiftCooldown = 0.45f;
        }
    }

    /// <summary>Cycles P-R-N-D only while the coach is fully stopped.</summary>
    public bool CycleGearSelection()
    {
        if (SpeedKph > 1f) return false;
        gearSelection = (GearSelection)(((int)gearSelection + 1) % 4);
        if (gearSelection == GearSelection.Drive) automaticGear = 1;
        return true;
    }

    public void SetAccessoryLoad(float load) { accessoryLoad = Mathf.Clamp01(load); }

    public void SetInputs(float steering, float throttle, float brake, bool applyHandbrake = false)
    {
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        throttleInput = Mathf.Clamp01(throttle);
        brakeInput = Mathf.Clamp01(brake);
        handbrake = applyHandbrake;
    }

    public void Configure(WheelCollider fl, WheelCollider fr, WheelCollider rl, WheelCollider rr,
        WheelCollider rli, WheelCollider rri, float driveTorque, float brakeTorque, float steeringAngle, float speedLimitKph)
    {
        frontLeft = fl; frontRight = fr; rearLeft = rl; rearRight = rr;
        rearLeftInner = rli; rearRightInner = rri;
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

    public void SetIndicators(bool left, bool right)
    {
        LeftIndicatorOn = left;
        RightIndicatorOn = right && !left;
        if (left || right) HazardLightsOn = false;
    }
    public void ToggleLeftIndicator() { bool next = !LeftIndicatorOn; SetIndicators(next, false); }
    public void ToggleRightIndicator() { bool next = !RightIndicatorOn; SetIndicators(false, next); }
    public void ToggleHazardLights()
    {
        HazardLightsOn = !HazardLightsOn;
        if (HazardLightsOn) { LeftIndicatorOn = false; RightIndicatorOn = false; }
    }
    public void ConfigureIndicatorLights(params Light[] lights)
    {
        indicatorLights = lights ?? System.Array.Empty<Light>();
        for (int i = 0; i < indicatorLights.Length; i++)
            if (indicatorLights[i] != null) indicatorLights[i].enabled = false;
    }
    private void UpdateExteriorIndicators()
    {
        bool flash = IndicatorsBlinking && Mathf.Repeat(Time.time, 0.8f) < 0.4f;
        for (int i = 0; i < indicatorLights.Length; i++)
        {
            Light lamp = indicatorLights[i];
            if (lamp == null) continue;
            bool isLeft = i == 0 || i == 2;
            bool active = HazardLightsOn || (isLeft ? LeftIndicatorOn : RightIndicatorOn);
            lamp.enabled = active && flash;
        }
    }

    public void ConfigureBrakeLights(params Light[] lights)
    {
        brakeLights = lights ?? System.Array.Empty<Light>();
        UpdateBrakeLights();
    }

    private void UpdateBrakeLights()
    {
        bool braking = brakeInput > 0.05f || handbrake || gearSelection == GearSelection.Park || airPressureBar < 1f;
        for (int i = 0; i < brakeLights.Length; i++)
            if (brakeLights[i] != null) brakeLights[i].enabled = braking;
    }


    public void ConfigureTailLights(params Light[] lights)
    {
        tailLights = lights ?? System.Array.Empty<Light>();
        UpdateTailLights();
    }

    private void UpdateTailLights()
    {
        for (int i = 0; i < tailLights.Length; i++)
            if (tailLights[i] != null) tailLights[i].enabled = HeadlightsOn;
    }

    public void ConfigureReverseLights(params Light[] lights)
    {
        reverseLights = lights ?? System.Array.Empty<Light>();
        UpdateReverseLights();
    }

    private void UpdateReverseLights()
    {
        bool reverseActive = EngineRunning && gearSelection == GearSelection.Reverse;
        for (int i = 0; i < reverseLights.Length; i++)
            if (reverseLights[i] != null) reverseLights[i].enabled = reverseActive;
    }

    public void ConfigureHeadlights(Light left, Light right)
    {
        leftHeadlight = left; rightHeadlight = right;
        if (leftHeadlight != null) leftHeadlightBaseIntensity = leftHeadlight.intensity;
        SetHeadlights(false);
    }
    public void ToggleHeadlights() { SetHeadlights(!HeadlightsOn); }
    private void SetHeadlights(bool enabled)
    {
        HeadlightsOn = enabled;
        if (!enabled) HighBeamOn = false;
        if (leftHeadlight != null) leftHeadlight.enabled = enabled;
        if (rightHeadlight != null) rightHeadlight.enabled = enabled;
        ApplyBeamIntensity();
        UpdateTailLights();
    }
    public void ToggleHighBeam()
    {
        if (!HeadlightsOn) return;
        SetHighBeam(!HighBeamOn);
    }
    public void SetHighBeam(bool enabled)
    {
        HighBeamOn = enabled;
        ApplyBeamIntensity();
    }
    private void ApplyBeamIntensity()
    {
        float intensity = leftHeadlightBaseIntensity * (HighBeamOn ? 1.65f : 1f);
        if (leftHeadlight != null) leftHeadlight.intensity = intensity;
        if (rightHeadlight != null) rightHeadlight.intensity = intensity;
    }
    public bool ToggleEngine()
    {
        if (EngineRunning)
        {
            if (SpeedKph > 1f) return false;
            EngineRunning = false;
            throttleInput = 0f;
            return true;
        }
        if (SpeedKph > 1f || fuelPercent <= 0f || PassengerDoorOpen || !SeatbeltFastened || (gearSelection != GearSelection.Park && gearSelection != GearSelection.Neutral)) return false;
        EngineRunning = true;
        return true;
    }
    public bool SetSeatbelt(bool fastened)
    {
        if (!fastened && SpeedKph > 1f) return false;
        SeatbeltFastened = fastened;
        return true;
    }

    /// <summary>Passenger doors may only be operated while the coach is fully stopped.</summary>
    public bool TogglePassengerDoor()
    {
        if (SpeedKph > 1f) return false;
        PassengerDoorOpen = !PassengerDoorOpen;
        return true;
    }
    public float GetMaximumRpm() { return 2400f + engineUpgradeLevel * 100f; }

    private void FixedUpdate()
    {
        if (!WheelsAssigned()) return;
        float speedFactor = Mathf.InverseLerp(maxSpeedKph, 0f, SpeedKph);
        bool safetyInterlock = PassengerDoorOpen || !SeatbeltFastened;
        float direction = gearSelection == GearSelection.Drive ? 1f : gearSelection == GearSelection.Reverse ? -1f : 0f;
        float ratio = gearSelection == GearSelection.Drive
            ? GearTorqueMultipliers[Mathf.Clamp(automaticGear, 1, 6)]
            : gearSelection == GearSelection.Reverse ? 1.45f : 0f;
        float rpmFraction = Mathf.InverseLerp(600f, GetMaximumRpm(), engineRpm);
        float torqueCurve = Mathf.Lerp(0.78f, 1.08f, Mathf.InverseLerp(0.05f, 0.58f, rpmFraction));
        if (rpmFraction > 0.88f) torqueCurve *= Mathf.Lerp(1f, 0.78f, Mathf.InverseLerp(0.88f, 1f, rpmFraction));
        torqueCurve *= 1f - accessoryLoad * 0.08f;
        float torque = EngineRunning && !safetyInterlock
            ? motorTorque * torqueCurve * throttleInput * speedFactor * ratio * direction : 0f;
        frontLeft.steerAngle = steeringInput * maxSteerAngle;
        frontRight.steerAngle = steeringInput * maxSteerAngle;
        rearLeft.motorTorque = torque * 0.25f;
        rearRight.motorTorque = torque * 0.25f;
        rearLeftInner.motorTorque = torque * 0.25f;
        rearRightInner.motorTorque = torque * 0.25f;
        float brakeAvailability = Mathf.InverseLerp(0f, 5f, airPressureBar);
        float requestedBrake = brakeInput * serviceBrakeTorque * brakeAvailability;
        absActive = false;
        float frontLeftBrake = ApplyAbs(frontLeft, requestedBrake);
        float frontRightBrake = ApplyAbs(frontRight, requestedBrake);
        float rearLeftService = ApplyAbs(rearLeft, requestedBrake);
        float rearRightService = ApplyAbs(rearRight, requestedBrake);
        float rearLeftInnerService = ApplyAbs(rearLeftInner, requestedBrake);
        float rearRightInnerService = ApplyAbs(rearRightInner, requestedBrake);
        float parkBrake = gearSelection == GearSelection.Park ? handbrakeTorque * 1.4f : 0f;
        float emergencySpringBrake = airPressureBar < 1f ? handbrakeTorque * 1.4f : 0f;
        float handbrakeForce = handbrake ? handbrakeTorque : 0f;
        frontLeft.brakeTorque = frontLeftBrake + parkBrake;
        frontRight.brakeTorque = frontRightBrake + parkBrake;
        rearLeft.brakeTorque = rearLeftService + handbrakeForce + parkBrake;
        rearRight.brakeTorque = rearRightService + handbrakeForce + parkBrake;
        rearLeftInner.brakeTorque = rearLeftInnerService + handbrakeForce + parkBrake;
        rearRightInner.brakeTorque = rearRightInnerService + handbrakeForce + parkBrake;
        body.AddForce(-transform.up * downforce * body.linearVelocity.magnitude);
    }


    private float ApplyAbs(WheelCollider wheel, float requestedTorque)
    {
        if (requestedTorque <= 0f || handbrake || gearSelection == GearSelection.Park) return requestedTorque;
        if (wheel == null || !wheel.GetGroundHit(out WheelHit hit)) return requestedTorque;

        float vehicleSpeed = body.linearVelocity.magnitude;
        if (vehicleSpeed < 2f) return requestedTorque;
        float wheelSurfaceSpeed = Mathf.Abs(wheel.rpm) * (2f * Mathf.PI * wheel.radius) / 60f;
        float slipRatio = (vehicleSpeed - wheelSurfaceSpeed) / Mathf.Max(vehicleSpeed, 1f);
        if (slipRatio <= 0.22f) return requestedTorque;

        absActive = true;
        return requestedTorque * 0.62f;
    }

    private bool WheelsAssigned() => frontLeft != null && frontRight != null && rearLeft != null && rearRight != null && rearLeftInner != null && rearRightInner != null;
}
