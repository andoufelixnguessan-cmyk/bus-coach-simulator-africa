using UnityEngine;

/// <summary>
/// Runtime quality presets designed around the 4 GB RAM Galaxy A07 baseline.
/// Values remain provisional until measured on the actual phone.
/// </summary>
public sealed class MobileGraphicsSettings : MonoBehaviour
{
    public enum Preset { Economy = 0, Balanced = 1, High = 2 }

    private const string PreferenceKey = "ATP_GRAPHICS_PRESET";
    public Preset CurrentPreset { get; private set; } = Preset.Economy;

    public string PresetLabel
    {
        get
        {
            switch (CurrentPreset)
            {
                case Preset.Balanced: return "ÉQUILIBRÉ";
                case Preset.High: return "ÉLEVÉ";
                default: return "ÉCONOMIE";
            }
        }
    }

    public void ApplySavedPreset()
    {
        CurrentPreset = (Preset)Mathf.Clamp(PlayerPrefs.GetInt(PreferenceKey, 0), 0, 2);
        Apply();
    }

    public void CyclePreset()
    {
        CurrentPreset = (Preset)(((int)CurrentPreset + 1) % 3);
        PlayerPrefs.SetInt(PreferenceKey, (int)CurrentPreset);
        PlayerPrefs.Save();
        Apply();
    }

    private void Apply()
    {
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.realtimeReflectionProbes = false;

        switch (CurrentPreset)
        {
            case Preset.Economy:
                Application.targetFrameRate = 30;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 0f;
                QualitySettings.lodBias = 0.65f;
                QualitySettings.maximumLODLevel = 1;
                QualitySettings.softParticles = false;
                break;

            case Preset.Balanced:
                Application.targetFrameRate = 30;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowDistance = 35f;
                QualitySettings.lodBias = 0.85f;
                QualitySettings.maximumLODLevel = 0;
                QualitySettings.softParticles = false;
                break;

            case Preset.High:
                Application.targetFrameRate = 30;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 65f;
                QualitySettings.lodBias = 1f;
                QualitySettings.maximumLODLevel = 0;
                QualitySettings.softParticles = true;
                break;
        }
    }
}
