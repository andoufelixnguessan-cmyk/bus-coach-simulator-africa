using System.Collections.Generic;
using UnityEngine;

/// <summary>Animated paired windshield wipers for single- and double-decker coaches.</summary>
public sealed class CoachWiperSystem : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float sweepSpeed = 2.2f;
    [SerializeField, Range(10f, 65f)] private float sweepAngle = 42f;
    private readonly List<Transform> wipers = new List<Transform>();
    private readonly List<Quaternion> restRotations = new List<Quaternion>();
    private bool enabledByDriver;

    public bool IsEnabled => enabledByDriver;

    public void Configure(params Transform[] pivots)
    {
        wipers.Clear();
        restRotations.Clear();
        if (pivots != null)
        {
            for (int i = 0; i < pivots.Length; i++)
            {
                if (pivots[i] == null) continue;
                wipers.Add(pivots[i]);
                restRotations.Add(pivots[i].localRotation);
            }
        }
        ApplyRestPose();
    }

    public void Toggle()
    {
        enabledByDriver = !enabledByDriver;
        if (!enabledByDriver) ApplyRestPose();
    }

    private void Update()
    {
        if (!enabledByDriver) return;
        float sweep = Mathf.Sin(Time.time * sweepSpeed * Mathf.PI * 2f) * sweepAngle;
        for (int i = 0; i < wipers.Count; i++)
        {
            float direction = i % 2 == 0 ? 1f : -1f;
            wipers[i].localRotation = restRotations[i] * Quaternion.Euler(0f, 0f, sweep * direction);
        }
    }

    private void ApplyRestPose()
    {
        for (int i = 0; i < wipers.Count; i++)
            if (wipers[i] != null) wipers[i].localRotation = restRotations[i];
    }
}