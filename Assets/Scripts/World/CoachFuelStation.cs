using System.Collections.Generic;
using UnityEngine;

/// <summary>Marks the service forecourt and enables refuelling only while the coach is inside it.</summary>
[RequireComponent(typeof(BoxCollider))]
public sealed class CoachFuelStation : MonoBehaviour
{
    private readonly Dictionary<CoachFuelSystem, int> overlaps = new Dictionary<CoachFuelSystem, int>();
    private void Awake()
    {
        BoxCollider area = GetComponent<BoxCollider>();
        area.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CoachFuelSystem fuel = other.GetComponentInParent<CoachFuelSystem>();
        if (fuel == null) return;
        int count;
        overlaps.TryGetValue(fuel, out count);
        overlaps[fuel] = count + 1;
        fuel.SetStationPresence(true);
    }

    private void OnDisable()
    {
        foreach (CoachFuelSystem fuel in overlaps.Keys)
            if (fuel != null) fuel.SetStationPresence(false);
        overlaps.Clear();
    }

    private void OnTriggerExit(Collider other)
    {
        CoachFuelSystem fuel = other.GetComponentInParent<CoachFuelSystem>();
        if (fuel == null) return;
        int count;
        if (!overlaps.TryGetValue(fuel, out count)) return;
        count--;
        if (count <= 0)
        {
            overlaps.Remove(fuel);
            fuel.SetStationPresence(false);
        }
        else overlaps[fuel] = count;
    }
}