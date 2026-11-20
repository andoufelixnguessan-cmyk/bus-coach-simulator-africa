using UnityEngine;

/// <summary>Functional refuelling at marked stations, paid from the saved company account.</summary>
public sealed class CoachFuelSystem : MonoBehaviour
{
    public const float TankCapacityLitres = 420f;
    public const long GamePricePerLitreXof = 750L;

    private CoachVehicleController vehicle;
    private CompanyProfile company;
    public bool AtFuelStation { get; private set; }
    public float FuelLitres => vehicle == null ? 0f : vehicle.FuelPercent * TankCapacityLitres / 100f;
    public long EstimatedFullRefillCostXof
    {
        get
        {
            if (vehicle == null) return 0L;
            float litresNeeded = (100f - vehicle.FuelPercent) * TankCapacityLitres / 100f;
            return (long)Mathf.Ceil(litresNeeded * GamePricePerLitreXof);
        }
    }

    public void Configure(CoachVehicleController target, CompanyProfile owner)
    {
        vehicle = target;
        company = owner;
    }

    public void SetStationPresence(bool inside)
    {
        AtFuelStation = inside;
    }

    public bool TryRefuelToFull(out string reason)
    {
        reason = string.Empty;
        if (vehicle == null || company == null)
        {
            reason = "Compagnie ou car introuvable.";
            return false;
        }
        if (!AtFuelStation)
        {
            reason = "Placez le car dans une station-service.";
            return false;
        }
        if (vehicle.EngineRunning || vehicle.SpeedKph > 1f || !vehicle.ParkingBrakeApplied)
        {
            reason = "Coupez le moteur, immobilisez le car et serrez le frein de stationnement.";
            return false;
        }
        long cost = EstimatedFullRefillCostXof;
        if (cost <= 0L)
        {
            reason = "Le réservoir est déjà plein.";
            return false;
        }
        if (!company.TrySpend(cost, "Carburant diesel - station-service"))
        {
            reason = "Solde insuffisant pour faire le plein.";
            return false;
        }
        vehicle.RefillFuelToFull();
        reason = "Plein effectué : " + cost.ToString("N0") + " FCFA.";
        return true;
    }
}