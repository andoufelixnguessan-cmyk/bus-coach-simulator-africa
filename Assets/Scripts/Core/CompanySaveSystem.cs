using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Local save/load for company progression, owned coaches and finance history.</summary>
public static class CompanySaveSystem
{
    private const string SaveKey = "BUS_COACH_SIM_AFRICA_COMPANY_V1";

    [Serializable]
    private sealed class SaveData
    {
        public int Version = 1;
        public long BalanceXof;
        public int CompanyLevel;
        public string StartingCoachId;
        public List<string> OwnedCoachIds = new List<string>();
        public List<FinanceEntry> FinanceHistory = new List<FinanceEntry>();
    }

    public static bool HasSave => PlayerPrefs.HasKey(SaveKey);

    public static void Save(CompanyProfile company)
    {
        if (company == null) return;
        SaveData data = new SaveData
        {
            BalanceXof = company.BalanceXof,
            CompanyLevel = company.CompanyLevel,
            StartingCoachId = company.StartingCoachId,
            OwnedCoachIds = company.OwnedCoachIds == null
                ? new List<string>() : new List<string>(company.OwnedCoachIds),
            FinanceHistory = company.FinanceHistory == null
                ? new List<FinanceEntry>() : new List<FinanceEntry>(company.FinanceHistory)
        };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool TryLoad(out CompanyProfile company)
    {
        company = null;
        if (!PlayerPrefs.HasKey(SaveKey)) return false;
        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
            if (data == null || data.Version != 1 || data.BalanceXof < 0L ||
                data.CompanyLevel < 1 || data.OwnedCoachIds == null)
                return false;

            List<string> validCoachIds = new List<string>();
            for (int i = 0; i < data.OwnedCoachIds.Count; i++)
            {
                string id = data.OwnedCoachIds[i];
                CoachDefinition definition = CoachEconomy.FindCoach(id);
                if (definition != null && definition.MinimumWheelCount >= 6 &&
                    !validCoachIds.Contains(id))
                    validCoachIds.Add(id);
            }
            if (validCoachIds.Count == 0) return false;

            List<FinanceEntry> history = data.FinanceHistory ?? new List<FinanceEntry>();
            company = CompanyProfile.RestoreFromSave(data.BalanceXof,
                data.CompanyLevel, validCoachIds, history, data.StartingCoachId);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Sauvegarde compagnie illisible : " + exception.Message);
            return false;
        }
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}