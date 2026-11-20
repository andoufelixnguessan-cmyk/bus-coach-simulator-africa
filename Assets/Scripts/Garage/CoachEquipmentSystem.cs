using System;
using System.Collections.Generic;

public enum CoachEquipmentId
{
    AirConditioning, Radio, PassengerTelevision, UsbCharging, Wifi, RecliningSeats, Footrests, Curtains,
    PassengerMicrophone, Refrigerator, CateringService, OnboardToilet
}

[Serializable]
public sealed class CoachEquipmentDefinition
{
    public CoachEquipmentId Id;
    public string DisplayName;
    public long PriceXof;
    public int MinimumPassengerSeats;
    public bool RequiresComfortOrVip;

    public CoachEquipmentDefinition(CoachEquipmentId id, string name, long price, int minSeats, bool comfortOrVip)
    { Id = id; DisplayName = name; PriceXof = price; MinimumPassengerSeats = minSeats; RequiresComfortOrVip = comfortOrVip; }
}

[Serializable]
public sealed class InstalledCoachEquipment
{
    public string CoachId;
    public CoachEquipmentId EquipmentId;
    public InstalledCoachEquipment(string coachId, CoachEquipmentId equipmentId)
    { CoachId = coachId; EquipmentId = equipmentId; }
}

/// <summary>
/// Compatibility and FCFA purchase rules for optional coach comfort equipment.
/// Safety equipment (seatbelts, emergency exits, extinguishers and first aid) is never sold here.
/// </summary>
[Serializable]
public sealed class CoachEquipmentSystem
{
    private static readonly CoachEquipmentDefinition[] Catalogue =
    {
        new CoachEquipmentDefinition(CoachEquipmentId.AirConditioning, "Climatisation", 1200000L, 16, false),
        new CoachEquipmentDefinition(CoachEquipmentId.Radio, "Radio et système audio", 180000L, 16, false),
        new CoachEquipmentDefinition(CoachEquipmentId.PassengerTelevision, "Écrans TV passagers", 650000L, 23, true),
        new CoachEquipmentDefinition(CoachEquipmentId.UsbCharging, "Prises USB passagers", 250000L, 16, false),
        new CoachEquipmentDefinition(CoachEquipmentId.Wifi, "Wi-Fi à bord", 300000L, 16, false),
        new CoachEquipmentDefinition(CoachEquipmentId.RecliningSeats, "Sièges inclinables", 900000L, 23, true),
        new CoachEquipmentDefinition(CoachEquipmentId.Footrests, "Repose-pieds", 280000L, 31, true),
        new CoachEquipmentDefinition(CoachEquipmentId.Curtains, "Rideaux individuels", 160000L, 23, true),
        new CoachEquipmentDefinition(CoachEquipmentId.PassengerMicrophone, "Microphone et annonces", 140000L, 16, false),
        new CoachEquipmentDefinition(CoachEquipmentId.Refrigerator, "Réfrigérateur de service", 420000L, 23, false),
        new CoachEquipmentDefinition(CoachEquipmentId.CateringService, "Équipement de restauration", 350000L, 23, false),
        new CoachEquipmentDefinition(CoachEquipmentId.OnboardToilet, "Toilettes à bord", 2800000L, 40, true)
    };

    public List<InstalledCoachEquipment> Installed { get; private set; }
    public IReadOnlyList<CoachEquipmentDefinition> AllEquipment => Catalogue;

    public CoachEquipmentSystem() { Installed = new List<InstalledCoachEquipment>(); }

    public static CoachEquipmentDefinition Find(CoachEquipmentId id)
    {
        for (int i = 0; i < Catalogue.Length; i++) if (Catalogue[i].Id == id) return Catalogue[i];
        return null;
    }

    public bool IsInstalled(string coachId, CoachEquipmentId equipmentId)
    {
        for (int i = 0; i < Installed.Count; i++)
            if (Installed[i].CoachId == coachId && Installed[i].EquipmentId == equipmentId) return true;
        return false;
    }

    public bool CanInstall(string coachId, CoachEquipmentId equipmentId, CompanyProfile company)
    {
        if (company == null || !company.OwnsCoach(coachId)) return false;
        CoachDefinition coach = CoachEconomy.FindCoach(coachId);
        CoachEquipmentDefinition item = Find(equipmentId);
        if (coach == null || item == null || IsInstalled(coachId, equipmentId)) return false;
        if (coach.MinSeats < item.MinimumPassengerSeats) return false;
        if (item.RequiresComfortOrVip && coach.HighestServiceClass < CoachServiceClass.Comfort) return false;
        return true;
    }

    public bool TryInstall(string coachId, CoachEquipmentId equipmentId, CompanyProfile company)
    {
        if (!CanInstall(coachId, equipmentId, company)) return false;
        CoachEquipmentDefinition item = Find(equipmentId);
        if (!company.TrySpend(item.PriceXof, "Garage : installation " + item.DisplayName)) return false;
        Installed.Add(new InstalledCoachEquipment(coachId, equipmentId));
        return true;
    }

    public bool HasAirConditioning(string coachId) { return IsInstalled(coachId, CoachEquipmentId.AirConditioning); }
    public bool HasPassengerTelevision(string coachId) { return IsInstalled(coachId, CoachEquipmentId.PassengerTelevision); }
    public bool HasRadio(string coachId) { return IsInstalled(coachId, CoachEquipmentId.Radio); }
    public bool HasCatering(string coachId) { return IsInstalled(coachId, CoachEquipmentId.CateringService); }
}