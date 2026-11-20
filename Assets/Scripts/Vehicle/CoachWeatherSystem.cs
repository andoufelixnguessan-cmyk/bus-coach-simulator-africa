using UnityEngine;

/// <summary>Toggleable rainfall, moving rain particles and wet-road material response.</summary>
public sealed class CoachWeatherSystem : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private ParticleSystem rainParticles;
    private ParticleSystem.EmissionModule rainEmission;
    public bool IsRaining { get; private set; }

    public void Configure(CoachVehicleController target)
    {
        vehicle = target;
        GameObject rainObject = new GameObject("Rainfall around coach");
        rainObject.transform.SetParent(vehicle != null ? vehicle.transform : transform, false);
        rainObject.transform.localPosition = new Vector3(0f, 8f, 0f);
        rainParticles = rainObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = rainParticles.main;
        main.loop = true;
        main.duration = 4f;
        main.startLifetime = 1.4f;
        main.startSpeed = 0f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.72f, 0.82f, 0.92f, 0.62f);
        main.maxParticles = 700;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;
        ParticleSystem.VelocityOverLifetimeModule fall = rainParticles.velocityOverLifetime;
        fall.enabled = true;
        fall.space = ParticleSystemSimulationSpace.World;
        fall.x = new ParticleSystem.MinMaxCurve(0.25f);
        fall.y = new ParticleSystem.MinMaxCurve(-17f);
        fall.z = new ParticleSystem.MinMaxCurve(0f);

        rainEmission = rainParticles.emission;
        rainEmission.rateOverTime = 0f;
        ParticleSystem.ShapeModule shape = rainParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(26f, 1f, 34f);

        ParticleSystemRenderer renderer = rainParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.StretchedBillboard;
        renderer.velocityScale = 0.18f;
        renderer.lengthScale = 2.2f;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        renderer.material = new Material(shader)
        {
            color = new Color(0.72f, 0.82f, 0.92f, 0.62f)
        };
        rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void ToggleRain()
    {
        SetRain(!IsRaining);
    }

    public void SetRain(bool enabled)
    {
        IsRaining = enabled;
        if (rainParticles == null) return;
        rainEmission.rateOverTime = enabled ? 260f : 0f;
        if (enabled) rainParticles.Play();
        else rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        AfricanRoadWorldBuilder.SetRoadWet(enabled);
    }

    private void OnDisable()
    {
        if (rainParticles != null) rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        AfricanRoadWorldBuilder.SetRoadWet(false);
    }
}