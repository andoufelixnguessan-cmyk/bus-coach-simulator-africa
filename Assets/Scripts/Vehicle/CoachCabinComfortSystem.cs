using UnityEngine;

/// <summary>Functional cabin climate control with engine accessory load and gradual temperature response.</summary>
public sealed class CoachCabinComfortSystem : MonoBehaviour
{
    private CoachVehicleController vehicle;
    [SerializeField, Range(18f, 28f)] private float coolingSetpointC = 22f;
    [SerializeField, Range(25f, 40f)] private float ambientTemperatureC = 32f;
    [SerializeField, Range(0.01f, 0.2f)] private float coolingRate = 0.065f;
    public bool AirConditioningOn { get; private set; }
    public float CabinTemperatureC { get; private set; } = 30f;

    public void Configure(CoachVehicleController target)
    {
        vehicle = target;
        UpdateAccessoryLoad();
    }

    public void ToggleAirConditioning()
    {
        AirConditioningOn = !AirConditioningOn;
        UpdateAccessoryLoad();
    }

    private void Update()
    {
        bool cooling = AirConditioningOn && vehicle != null && vehicle.EngineRunning;
        float target = cooling ? coolingSetpointC : ambientTemperatureC;
        CabinTemperatureC = Mathf.MoveTowards(CabinTemperatureC, target,
            Time.deltaTime * (cooling ? coolingRate : coolingRate * 0.22f));
        UpdateAccessoryLoad();
    }

    private void UpdateAccessoryLoad()
    {
        if (vehicle != null)
            vehicle.SetAccessoryLoad(AirConditioningOn && vehicle.EngineRunning ? 0.22f : 0f);
    }

    private void OnDisable()
    {
        if (vehicle != null) vehicle.SetAccessoryLoad(0f);
    }
}