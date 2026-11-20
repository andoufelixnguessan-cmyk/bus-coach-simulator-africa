using System;
using UnityEngine;

/// <summary>Original per-coach instrument and diesel-engine tuning. Replace placeholder values with licensed vehicle data.</summary>
[Serializable]
public sealed class CoachDashboardProfile
{
    public string CoachId;
    public string DisplayName;
    public string EngineFamily;
    public float IdleRpm;
    public float RedlineRpm;
    public float EngineToneHz;
    public float EngineVolume;
    public float EngineRumble;
    public bool HasDigitalCluster;

    public CoachDashboardProfile(string id, string name, string engine, float idle, float redline,
        float tone, float volume, float rumble, bool digital)
    {
        CoachId = id; DisplayName = name; EngineFamily = engine;
        IdleRpm = idle; RedlineRpm = redline; EngineToneHz = tone;
        EngineVolume = volume; EngineRumble = rumble; HasDigitalCluster = digital;
    }

    public static CoachDashboardProfile ForCoach(string coachId)
    {
        switch (coachId)
        {
            case "autocar-standard":
                return new CoachDashboardProfile(coachId, "Autocar standard", "Diesel 6 cylindres", 600, 2350, 47f, 0.22f, 0.65f, false);
            case "autocar-confort":
                return new CoachDashboardProfile(coachId, "Autocar confort", "Diesel 6 cylindres turbo", 620, 2450, 53f, 0.20f, 0.52f, true);
            case "grand-tourisme":
                return new CoachDashboardProfile(coachId, "Grand tourisme", "Diesel turbo longue distance", 600, 2500, 58f, 0.19f, 0.48f, true);
            case "autocar-premium":
                return new CoachDashboardProfile(coachId, "Autocar premium", "Diesel turbo haute puissance", 580, 2550, 64f, 0.18f, 0.42f, true);
            case "double-etage-tourisme":
                return new CoachDashboardProfile(coachId, "Double étage tourisme", "Diesel grande capacité", 590, 2300, 42f, 0.25f, 0.78f, true);
            default:
                return new CoachDashboardProfile("car-moyen", "Car interurbain", "Diesel 6 cylindres", 650, 2400, 49f, 0.24f, 0.72f, false);
        }
    }
}
