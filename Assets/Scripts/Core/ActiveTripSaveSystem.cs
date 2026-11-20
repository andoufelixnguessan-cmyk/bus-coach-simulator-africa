using System;
using UnityEngine;

/// <summary>
/// Serializable snapshot for an active trip. It deliberately stores domain data rather than
/// Unity object references so a fresh session can be rebuilt safely after the app restarts.
/// </summary>
[Serializable]
public sealed class PlayerTripSaveData
{
    public const int CurrentVersion = 1;

    public int SaveVersion;
    public string TripId;
    public string CoachId;
    public string RouteId;
    public string RouteDisplayName;
    public int EstimatedDistanceKm;
    public int EstimatedDrivingMinutes;
    public int ServiceClass;
    public RouteStop[] RouteStops;
    public BorderCheckpoint[] RouteBorders;
    public PassengerBooking[] PassengerBookings;
    public int NextTicketNumber;
    public int LastProcessedPassengerStopIndex;
    public long TicketRevenueXof;
    public int NextStopIndex;
    public BorderCrossingRecord[] BorderRecords;
    public int TripStatus;
    public long ChargedOperatingCostsXof;
    public long SafeDrivingBonusXof;
    public long InternationalProfitGuaranteeXof;
    public bool IsRaining;
    public bool IsNight;
    public long SavedAtUtcTicks;
}

/// <summary>Local active-trip persistence. A completed or cancelled trip is never resumed.</summary>
public static class ActiveTripSaveSystem
{
    private const string SaveKey = "ATP_ACTIVE_TRIP_SAVE_V1";

    public static bool HasActiveTripSave
    {
        get { return PlayerPrefs.HasKey(SaveKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(SaveKey)); }
    }

    public static bool Save(PlayerTripSession session)
    {
        return Save(session, false, false);
    }

    public static bool Save(PlayerTripSession session, bool isRaining, bool isNight)
    {
        if (session == null || session.Status != CoachTripStatus.OnRoad)
        {
            Clear();
            return false;
        }

        try
        {
            PlayerTripSaveData data = new PlayerTripSaveData();
            session.CaptureSaveState(data);
            data.IsRaining = isRaining;
            data.IsNight = isNight;
            data.SavedAtUtcTicks = DateTime.UtcNow.Ticks;
            string json = JsonUtility.ToJson(data);
            if (string.IsNullOrEmpty(json)) return false;
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Échec de sauvegarde du voyage actif : " + exception.Message);
            return false;
        }
    }

    public static bool TryRestore(CompanyProfile company, out PlayerTripSession session, out string reason)
    {
        bool ignoredRain, ignoredNight;
        return TryRestore(company, out session, out reason, out ignoredRain, out ignoredNight);
    }

    public static bool TryRestore(CompanyProfile company, out PlayerTripSession session,
        out string reason, out bool isRaining, out bool isNight)
    {
        isRaining = false;
        isNight = false;
        session = null;
        reason = string.Empty;
        if (!HasActiveTripSave)
        {
            reason = "Aucun voyage actif sauvegardé.";
            return false;
        }

        try
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            PlayerTripSaveData data = JsonUtility.FromJson<PlayerTripSaveData>(json);
            if (!PlayerTripSession.TryRestoreFromSave(data, company, out session, out reason))
            {
                Debug.LogWarning("Reprise du voyage impossible : " + reason);
                return false;
            }
            isRaining = data.IsRaining;
            isNight = data.IsNight;
            return true;
        }
        catch (Exception exception)
        {
            reason = "La sauvegarde du voyage est illisible : " + exception.Message;
            Debug.LogError(reason);
            return false;
        }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
