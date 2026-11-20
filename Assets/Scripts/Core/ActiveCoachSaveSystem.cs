using System;
using UnityEngine;

/// <summary>
/// Crash/pause-resistant local checkpoint for the player's active coach.
/// The coach is restored at its exact saved world transform; it resumes parked with the engine off.
/// </summary>
public static class ActiveCoachSaveSystem
{
    private const string SaveKey = "BUS_COACH_SIM_AFRICA_ACTIVE_COACH_V1";

    [Serializable]
    private sealed class SaveData
    {
        public int Version = 1;
        public bool HasActiveCoach;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        public float FuelPercent = 86f;
        public string SavedAtUtc;
    }

    public static bool HasActiveCoachSave
    {
        get
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return false;
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
                return data != null && data.Version == 1 && data.HasActiveCoach;
            }
            catch { return false; }
        }
    }

    public static void Save(CoachVehicleController coach)
    {
        if (coach == null) return;
        SaveData data = new SaveData
        {
            HasActiveCoach = true,
            Position = coach.transform.position,
            Rotation = coach.transform.rotation,
            FuelPercent = coach.FuelPercent,
            SavedAtUtc = DateTime.UtcNow.ToString("o")
        };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool TryRestore(CoachVehicleController coach)
    {
        if (coach == null || !PlayerPrefs.HasKey(SaveKey)) return false;
        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
            if (data == null || data.Version != 1 || !data.HasActiveCoach ||
                float.IsNaN(data.FuelPercent) || float.IsInfinity(data.FuelPercent) ||
                data.FuelPercent < 0f || data.FuelPercent > 100f)
                return false;

            coach.RestoreSavedState(data.Position, data.Rotation, Vector3.zero, data.FuelPercent);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Point de reprise du car illisible : " + exception.Message);
            return false;
        }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
