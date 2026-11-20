using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pointer-down/up handler for a single mobile driving control.
/// </summary>
public sealed class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ActionType { SteerLeft, SteerRight, Throttle, Brake, Handbrake }

    private MobileDriveInput input;
    private ActionType action;
    private Image buttonImage;
    private Color normalColor;
    private bool hasNormalColor;

    public void Configure(MobileDriveInput target, ActionType controlAction)
    {
        input = target;
        action = controlAction;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null) { normalColor = buttonImage.color; hasNormalColor = true; }
    }

    public void OnPointerDown(PointerEventData eventData) { Apply(true); SetVisualPressed(true); }
    public void OnPointerUp(PointerEventData eventData) { Apply(false); SetVisualPressed(false); }
    public void OnPointerExit(PointerEventData eventData) { Apply(false); SetVisualPressed(false); }

    private void OnDisable() { Apply(false); SetVisualPressed(false); }

    private void SetVisualPressed(bool pressed)
    {
        if (buttonImage == null || !hasNormalColor) return;
        buttonImage.color = pressed ? Color.Lerp(normalColor, Color.white, 0.24f) : normalColor;
    }

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
