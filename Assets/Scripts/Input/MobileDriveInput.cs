using UnityEngine;

/// <summary>
/// Input state for touch controls. Connect UI pointer-down/up events
/// to the press/release methods and feed these values to CoachVehicleController.
/// </summary>
public sealed class MobileDriveInput : MonoBehaviour
{
    public float Steering { get; private set; }
    public float Throttle { get; private set; }
    public float Brake { get; private set; }
    public bool Handbrake { get; private set; }

    public void SetSteering(float value) => Steering = Mathf.Clamp(value, -1f, 1f);
    public void ReleaseSteering() => Steering = 0f;

    public void PressThrottle() => Throttle = 1f;
    public void ReleaseThrottle() => Throttle = 0f;

    public void PressBrake() => Brake = 1f;
    public void ReleaseBrake() => Brake = 0f;

    public void PressHandbrake() => Handbrake = true;
    public void ReleaseHandbrake() => Handbrake = false;

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ResetInputs();
    }

    private void OnDisable() => ResetInputs();

    private void ResetInputs()
    {
        Steering = 0f;
        Throttle = 0f;
        Brake = 0f;
        Handbrake = false;
    }
}