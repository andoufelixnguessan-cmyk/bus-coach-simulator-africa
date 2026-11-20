using UnityEngine;

/// <summary>Controls the installed passenger-cabin lights on the active coach.</summary>
public sealed class CoachCabinLightingSystem : MonoBehaviour
{
    private Light[] cabinLights = System.Array.Empty<Light>();
    public bool IsOn { get; private set; }

    public void Configure(params Light[] lights)
    {
        cabinLights = lights ?? System.Array.Empty<Light>();
        SetEnabled(false);
    }

    public void Toggle() => SetEnabled(!IsOn);

    public void SetEnabled(bool enabled)
    {
        IsOn = enabled;
        for (int i = 0; i < cabinLights.Length; i++)
            if (cabinLights[i] != null) cabinLights[i].enabled = enabled;
    }

    private void OnDisable() => SetEnabled(false);
}