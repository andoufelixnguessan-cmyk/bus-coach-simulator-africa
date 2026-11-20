using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Temporary Canvas-based multi-touch HUD for the first Android driving prototype.
/// The final release will replace this layout with the finished cockpit interface.
/// </summary>
public sealed class PrototypeTouchControls : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private MobileDriveInput input;
    private Text speedText;
    private int lastSpeedRefreshFrame;

    public void Configure(CoachVehicleController target, MobileDriveInput driveInput)
    {
        vehicle = target;
        input = driveInput;
        BuildCanvas();
    }

    private void Update()
    {
        if (vehicle == null || speedText == null || Time.frameCount - lastSpeedRefreshFrame < 6)
            return;

        lastSpeedRefreshFrame = Time.frameCount;
        speedText.text = "VITESSE  " + Mathf.RoundToInt(vehicle.SpeedKph) + " km/h";
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Mobile Driving HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (EventSystem.current == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(canvasObject.transform, false);
        }

        CreateHoldButton(canvas.transform, "◀", MobileHoldButton.ActionType.SteerLeft,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(42f, 42f), new Vector2(142f, 112f));
        CreateHoldButton(canvas.transform, "▶", MobileHoldButton.ActionType.SteerRight,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(196f, 42f), new Vector2(142f, 112f));
        CreateHoldButton(canvas.transform, "FREIN", MobileHoldButton.ActionType.Brake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-344f, 42f), new Vector2(142f, 112f));
        CreateHoldButton(canvas.transform, "ACCELERER", MobileHoldButton.ActionType.Throttle,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-190f, 42f), new Vector2(160f, 112f));
        CreateHoldButton(canvas.transform, "FREIN PARK", MobileHoldButton.ActionType.Handbrake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-190f, 174f), new Vector2(160f, 78f));

        var speedPanel = CreatePanel(canvas.transform, "Speed panel",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -38f), new Vector2(310f, 70f),
            new Color(0.025f, 0.055f, 0.09f, 0.78f));
        speedText = CreateText(speedPanel.transform, "VITESSE  0 km/h", 28, TextAnchor.MiddleLeft);
        speedText.rectTransform.offsetMin = new Vector2(18f, 0f);
        speedText.rectTransform.offsetMax = new Vector2(-8f, 0f);
    }

    private void CreateHoldButton(Transform parent, string label, MobileHoldButton.ActionType action,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var panel = CreatePanel(parent, label + " button", anchor, pivot, position, size,
            new Color(0.025f, 0.055f, 0.09f, 0.72f));
        panel.AddComponent<MobileHoldButton>().Configure(input, action);
        CreateText(panel.transform, label, 25, TextAnchor.MiddleCenter);
    }

    private static GameObject CreatePanel(Transform parent, string objectName,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color)
    {
        var panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return panel;
    }

    private static Text CreateText(Transform parent, string value, int fontSize, TextAnchor alignment)
    {
        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        var rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }
}
