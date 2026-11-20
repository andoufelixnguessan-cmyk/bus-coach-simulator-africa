using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pointer-down/up handler for a single mobile driving control.
/// </summary>
public sealed class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ActionType { SteerLeft, SteerRight, Throttle, Brake, Handbrake }

    private MobileDriveInput input;
    private ActionType action;

    public void Configure(MobileDriveInput target, ActionType controlAction)
    {
        input = target;
        action = controlAction;
    }

    public void OnPointerDown(PointerEventData eventData) => Apply(true);
    public void OnPointerUp(PointerEventData eventData) => Apply(false);
    public void OnPointerExit(PointerEventData eventData) => Apply(false);

    private void OnDisable() => Apply(false);

    private void Apply(bool pressed)
    {
        if (input == null) return;

        switch (action)
        {
            case ActionType.SteerLeft: input.SetLeftHeld(pressed); break;
            case ActionType.SteerRight: input.SetRightHeld(pressed); break;
            case ActionType.Throttle:
                if (pressed) input.PressThrottle(); else input.ReleaseThrottle();
                break;
            case ActionType.Brake:
                if (pressed) input.PressBrake(); else input.ReleaseBrake();
                break;
            case ActionType.Handbrake:
                if (pressed) input.PressHandbrake(); else input.ReleaseHandbrake();
                break;
        }
    }
}
