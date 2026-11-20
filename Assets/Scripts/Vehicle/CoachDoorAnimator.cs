using UnityEngine;

/// <summary>Animated passenger door visual linked to the coach safety interlock.</summary>
public sealed class CoachDoorAnimator : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private Transform doorPanel;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    [SerializeField, Min(0.1f)] private float animationSpeed = 2.8f;

    public void Configure(CoachVehicleController target, Transform panel)
    {
        vehicle = target;
        doorPanel = panel;
        if (doorPanel != null)
        {
            closedRotation = doorPanel.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, -72f, 0f);
        }
    }

    private void Update()
    {
        if (vehicle == null || doorPanel == null) return;
        Quaternion target = vehicle.PassengerDoorOpen ? openRotation : closedRotation;
        doorPanel.localRotation = Quaternion.Slerp(doorPanel.localRotation, target, Time.deltaTime * animationSpeed);
    }
}
