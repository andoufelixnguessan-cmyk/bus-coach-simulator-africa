using UnityEngine;

/// <summary>Marks the service forecourt and enables refuelling only while the coach is inside it.</summary>
[RequireComponent(typeof(BoxCollider))]
public sealed class CoachFuelStation : MonoBehaviour
{
    private void Awake()
    {
        BoxCollider area = GetComponent<BoxCollider>();
        area.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CoachFuelSystem fuel = other.GetComponentInParent<CoachFuelSystem>();
        if (fuel != null) fuel.SetStationPresence(true);
    }

    private void OnTriggerExit(Collider other)
    {
        CoachFuelSystem fuel = other.GetComponentInParent<CoachFuelSystem>();
        if (fuel != null) fuel.SetStationPresence(false);
    }
}