using UnityEngine;

/// <summary>
/// Shared mobile driving input state. Separate left/right holds allow
/// simultaneous multi-touch steering and throttle/brake input.
/// </summary>
public sealed class MobileDriveInput : MonoBehaviour
{
    private bool leftHeld;
    private bool rightHeld;

    public float Steering { get; private set; }
    public float Throttle { get; private set; }
    public float Brake { get; private set; }
    public bool Handbrake { get; private set; }

    public void SetSteering(float value)
    {
        leftHeld = false;
        rightHeld = false;
        Steering = Mathf.Clamp(value, -1f, 1f);
    }

    public void ReleaseSteering() => SetSteering(0f);

    public void SetLeftHeld(bool held)
    {
        leftHeld = held;
        RefreshSteering();
    }

    public void SetRightHeld(bool held)
    {
        rightHeld = held;
        RefreshSteering();
    }

    private void RefreshSteering()
    {
        Steering = (rightHeld ? 1f : 0f) - (leftHeld ? 1f : 0f);
    }

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
        leftHeld = false;
        rightHeld = false;
        Steering = 0f;
        Throttle = 0f;
        Brake = 0f;
        Handbrake = false;
    }
}
