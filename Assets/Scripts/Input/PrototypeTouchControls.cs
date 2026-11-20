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
    private MobileGraphicsSettings graphics;
    private Text speedText;
    private Text qualityText;
    private int lastHudRefreshFrame;
    private RectTransform safeAreaRoot;

    public void Configure(CoachVehicleController target, MobileDriveInput driveInput, MobileGraphicsSettings graphicsSettings)
    {
        vehicle = target;
        input = driveInput;
        graphics = graphicsSettings;
        BuildCanvas();
    }

    private void Update()
    {
        if (Time.frameCount - lastHudRefreshFrame < 6) return;
        lastHudRefreshFrame = Time.frameCount;

        if (vehicle != null && speedText != null)
            speedText.text = "VITESSE  " + Mathf.RoundToInt(vehicle.SpeedKph) + " km/h";

        if (graphics != null && qualityText != null)
            qualityText.text = "GRAPHISMES  " + graphics.PresetLabel;
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Mobile Driving HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        GameObject safeAreaObject = new GameObject("Safe Area", typeof(RectTransform), typeof(MobileSafeAreaFitter));
        safeAreaObject.transform.SetParent(canvas.transform, false);
        safeAreaRoot = safeAreaObject.GetComponent<RectTransform>();
        safeAreaRoot.anchorMin = Vector2.zero;
        safeAreaRoot.anchorMax = Vector2.one;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        CreateHoldButton(safeAreaRoot, "◀", MobileHoldButton.ActionType.SteerLeft,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeAreaRoot, "▶", MobileHoldButton.ActionType.SteerRight,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(154f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeAreaRoot, "FREIN", MobileHoldButton.ActionType.Brake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-286f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeAreaRoot, "ACCÉLÉRER", MobileHoldButton.ActionType.Throttle,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152f, 26f), new Vector2(128f, 92f));
        CreateHoldButton(safeAreaRoot, "FREIN PARK", MobileHoldButton.ActionType.Handbrake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152f, 132f), new Vector2(128f, 66f));

        var speedPanel = CreatePanel(safeAreaRoot, "Speed panel",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(286f, 62f),
            new Color(0.025f, 0.055f, 0.09f, 0.9f));
        speedText = CreateText(speedPanel.transform, "VITESSE  0 km/h", 25, TextAnchor.MiddleLeft);
        speedText.rectTransform.offsetMin = new Vector2(16f, 0f);
        speedText.rectTransform.offsetMax = new Vector2(-8f, 0f);

        var titlePanel = CreatePanel(safeAreaRoot, "Game title",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(430f, 48f),
            new Color(0.025f, 0.055f, 0.09f, 0.72f));
        Text title = CreateText(titlePanel.transform, "BUS & COACH SIMULATOR AFRICA", 18, TextAnchor.MiddleCenter);
        title.color = new Color(0.88f, 0.93f, 1f);

        var qualityPanel = CreatePanel(safeAreaRoot, "Graphics preset button",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(286f, 62f),
            new Color(0.025f, 0.055f, 0.09f, 0.9f));
        var qualityButton = qualityPanel.AddComponent<Button>();
        qualityButton.targetGraphic = qualityPanel.GetComponent<Image>();
        qualityButton.onClick.AddListener(CycleGraphics);
        qualityText = CreateText(qualityPanel.transform, "GRAPHISMES  ÉCONOMIE", 19, TextAnchor.MiddleCenter);
    }

    private void CycleGraphics()
    {
        if (graphics != null) graphics.CyclePreset();
    }

    private void CreateHoldButton(Transform parent, string label, MobileHoldButton.ActionType action,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var panel = CreatePanel(parent, label + " button", anchor, pivot, position, size,
            action == MobileHoldButton.ActionType.Throttle
                ? new Color(0.04f, 0.34f, 0.27f, 0.94f)
                : action == MobileHoldButton.ActionType.Brake || action == MobileHoldButton.ActionType.Handbrake
                    ? new Color(0.48f, 0.16f, 0.13f, 0.94f)
                    : new Color(0.08f, 0.22f, 0.38f, 0.94f));
        panel.AddComponent<MobileHoldButton>().Configure(input, action);
        Text labelText = CreateText(panel.transform, label, 20, TextAnchor.MiddleCenter);
        labelText.resizeTextForBestFit = true;
        labelText.resizeTextMinSize = 14;
        labelText.resizeTextMaxSize = 22;
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
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 13;
        text.resizeTextMaxSize = fontSize;
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
