using UnityEngine;

/// <summary>
/// Lightweight synthesized diesel loop for the prototype. Production audio should use original,
/// recorded/licensed engine samples per vehicle; this procedural layer is a temporary fallback.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public sealed class CoachEngineAudio : MonoBehaviour
{
    private AudioSource source;
    private CoachVehicleController vehicle;
    private CoachDashboardProfile profile;
    private volatile float rpm = 650f;
    private volatile float load;
    private float phaseA, phaseB, phaseC;
    private const int SampleRate = 22050;

    public void Configure(CoachVehicleController target, CoachDashboardProfile engineProfile)
    {
        vehicle = target;
        profile = engineProfile;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 1f;
        source.minDistance = 2.5f;
        source.maxDistance = 55f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.dopplerLevel = 0.15f;
        source.volume = profile.EngineVolume;
        source.clip = AudioClip.Create("Procedural diesel - " + profile.CoachId, SampleRate, 1, SampleRate, true, OnAudioRead);
        source.Play();
    }

    private void Update()
    {
        if (vehicle == null || profile == null) return;
        if (!vehicle.EngineRunning)
        {
            rpm = 0f;
            load = 0f;
            source.volume = 0f;
            return;
        }
        rpm = vehicle.EngineRpm;
        load = Mathf.Clamp01((vehicle.EngineRpmNormalized * 0.35f) + 0.15f + vehicle.AccessoryLoad * 0.25f);
        int upgrade = Mathf.Clamp(Mathf.RoundToInt((vehicle.GetMaximumRpm() - 2400f) / 100f), 0, 5);
        source.pitch = Mathf.Clamp(0.82f + (rpm / profile.RedlineRpm) * 0.48f + upgrade * 0.018f, 0.75f, 1.5f);
        source.volume = profile.EngineVolume * (0.72f + load * 0.28f);
    }

    private void OnAudioRead(float[] data)
    {
        CoachDashboardProfile p = profile;
        float baseTone = p == null ? 49f : p.EngineToneHz;
        float rumble = p == null ? 0.6f : p.EngineRumble;
        float frequency = baseTone * Mathf.Clamp(rpm / 850f, 0.72f, 2.9f);
        float amplitude = rpm <= 1f ? 0f : 0.12f + load * 0.12f;
        for (int i = 0; i < data.Length; i++)
        {
            phaseA += 2f * Mathf.PI * frequency / SampleRate;
            phaseB += 2f * Mathf.PI * frequency * 2.03f / SampleRate;
            phaseC += 2f * Mathf.PI * frequency * 3.97f / SampleRate;
            if (phaseA > Mathf.PI * 2f) phaseA -= Mathf.PI * 2f;
            if (phaseB > Mathf.PI * 2f) phaseB -= Mathf.PI * 2f;
            if (phaseC > Mathf.PI * 2f) phaseC -= Mathf.PI * 2f;
            float diesel = Mathf.Sin(phaseA) * 0.55f + Mathf.Sin(phaseB) * 0.28f + Mathf.Sin(phaseC) * 0.17f;
            data[i] = diesel * amplitude * rumble;
        }
    }

    private void OnDisable() { if (source != null && source.isPlaying) source.Stop(); }
}
