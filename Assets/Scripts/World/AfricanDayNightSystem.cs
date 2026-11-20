using UnityEngine;

/// <summary>Simple playable day/night lighting mode for the African road environment.</summary>
public sealed class AfricanDayNightSystem : MonoBehaviour
{
    public bool IsNight { get; private set; }

    public void Configure()
    {
        SetNight(false);
    }

    public void ToggleTimeOfDay()
    {
        SetNight(!IsNight);
    }

    public void SetNight(bool night)
    {
        IsNight = night;
        AfricanRoadWorldBuilder.SetNightLighting(night);
    }
}