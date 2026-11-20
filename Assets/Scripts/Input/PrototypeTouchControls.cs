using UnityEngine;

/// <summary>
/// Temporary touch HUD for the first playable prototype. Replace with a polished
/// Canvas-based control layout before release.
/// </summary>
public sealed class PrototypeTouchControls : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private MobileDriveInput input;
    private GUIStyle buttonStyle;
    private GUIStyle infoStyle;
    private GUIStyle labelStyle;
    private int cachedScreenHeight = -1;

    public void Configure(CoachVehicleController target, MobileDriveInput driveInput)
    {
        vehicle = target;
        input = driveInput;
    }

    private void Update()
    {
        if (vehicle != null && input != null)
            vehicle.SetInputs(input.Steering, input.Throttle, input.Brake, input.Handbrake);
    }

    private void OnGUI()
    {
        if (vehicle == null || input == null) return;

        float scale = Mathf.Clamp(Screen.height / 720f, 0.75f, 1.25f);
        EnsureStyles(scale);

        int buttonWidth = Mathf.RoundToInt(88f * scale);
        int buttonHeight = Mathf.RoundToInt(72f * scale);
        int margin = Mathf.RoundToInt(28f * scale);
        int bottom = Screen.height - buttonHeight - Mathf.RoundToInt(74f * scale);

        bool left = GUI.RepeatButton(new Rect(margin, bottom, buttonWidth, buttonHeight), "◀", buttonStyle);
        bool right = GUI.RepeatButton(new Rect(margin + buttonWidth + 12f, bottom, buttonWidth, buttonHeight), "▶", buttonStyle);
        bool brake = GUI.RepeatButton(new Rect(Screen.width - margin - buttonWidth * 2f - 12f, bottom, buttonWidth, buttonHeight), "FREIN", buttonStyle);
        bool throttle = GUI.RepeatButton(new Rect(Screen.width - margin - buttonWidth, bottom, buttonWidth, buttonHeight), "GAZ", buttonStyle);
        bool handbrake = GUI.RepeatButton(new Rect(Screen.width - margin - buttonWidth, bottom - buttonHeight - 12f, buttonWidth, buttonHeight * 0.7f), "PARK", buttonStyle);

        input.SetSteering(left == right ? 0f : (left ? -1f : 1f));
        if (throttle) input.PressThrottle(); else input.ReleaseThrottle();
        if (brake) input.PressBrake(); else input.ReleaseBrake();
        if (handbrake) input.PressHandbrake(); else input.ReleaseHandbrake();

        GUI.Box(new Rect(margin, margin, 190f * scale, 42f * scale), "VITESSE  " + Mathf.RoundToInt(vehicle.SpeedKph) + " km/h", infoStyle);
        GUI.Label(new Rect(margin, margin + 46f * scale, 280f * scale, 32f * scale), "PROTOTYPE • MODE CONDUITE", labelStyle);
    }

    private void EnsureStyles(float scale)
    {
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            infoStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 14, 8, 8)
            };
            labelStyle = new GUIStyle(GUI.skin.label);
        }

        if (cachedScreenHeight == Screen.height) return;
        cachedScreenHeight = Screen.height;
        buttonStyle.fontSize = Mathf.RoundToInt(22f * scale);
        infoStyle.fontSize = Mathf.RoundToInt(18f * scale);
        labelStyle.fontSize = Mathf.RoundToInt(14f * scale);
    }
}
