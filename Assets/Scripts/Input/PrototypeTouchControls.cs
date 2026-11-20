using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Landscape-first mobile driving HUD with compact, working vehicle controls.</summary>
public sealed class PrototypeTouchControls : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private CoachWiperSystem wipers;
    private CoachCabinLightingSystem cabinLights;
    private CoachCabinComfortSystem cabinComfort;
    private MobileDriveInput input;
    private MobileGraphicsSettings graphics;
    private Text speedText;
    private Text qualityText;
    private Text gearText;
    private Image gearPanelImage;
    private CoachHudIcon seatbeltIcon;
    private CoachHudIcon doorIcon;
    private Image leftIndicatorImage, hazardImage, rightIndicatorImage, headlightImage, highBeamImage, engineImage;
    private Image seatbeltPanelImage, doorPanelImage, wiperImage, cabinLightImage, acImage;
    private int lastHudRefreshFrame;
    private CameraFollow cameraView;
    private Action openRoutePlanner;

    public void Configure(CoachVehicleController target, MobileDriveInput driveInput,
        MobileGraphicsSettings graphicsSettings, CameraFollow camera, CoachWiperSystem wiperSystem, CoachCabinLightingSystem cabinLightingSystem, CoachCabinComfortSystem cabinComfortSystem, Action showRoutePlanner)
    {
        vehicle = target;
        wipers = wiperSystem;
        cabinLights = cabinLightingSystem;
        cabinComfort = cabinComfortSystem;
        input = driveInput;
        graphics = graphicsSettings;
        cameraView = camera;
        openRoutePlanner = showRoutePlanner;
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
        if (vehicle != null && gearText != null)
            gearText.text = vehicle.CurrentGearLabel;
        if (vehicle != null && seatbeltIcon != null)
            seatbeltIcon.SetIcon(vehicle.SeatbeltFastened ? CoachHudIcon.IconType.SeatbeltOn : CoachHudIcon.IconType.SeatbeltOff);
        if (vehicle != null && doorIcon != null)
            doorIcon.SetIcon(vehicle.PassengerDoorOpen ? CoachHudIcon.IconType.DoorOpen : CoachHudIcon.IconType.DoorClosed);
        if (vehicle != null)
        {
            SetButtonState(leftIndicatorImage, vehicle.LeftIndicatorOn);
            SetButtonState(hazardImage, vehicle.HazardLightsOn);
            SetButtonState(rightIndicatorImage, vehicle.RightIndicatorOn);
            SetButtonState(headlightImage, vehicle.HeadlightsOn);
            SetButtonState(highBeamImage, vehicle.HighBeamOn);
            SetButtonState(engineImage, vehicle.EngineRunning);
            SetButtonState(seatbeltPanelImage, vehicle.SeatbeltFastened);
            SetButtonState(doorPanelImage, vehicle.PassengerDoorOpen);
            SetButtonState(gearPanelImage, vehicle.SelectedGear == CoachVehicleController.GearSelection.Drive || vehicle.SelectedGear == CoachVehicleController.GearSelection.Reverse);
        }
        if (wipers != null) SetButtonState(wiperImage, wipers.IsEnabled);
        if (cabinLights != null) SetButtonState(cabinLightImage, cabinLights.IsOn);
        if (cabinComfort != null) SetButtonState(acImage, cabinComfort.AirConditioningOn);
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Mobile Driving HUD", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        GameObject safeAreaObject = new GameObject("Safe Area", typeof(RectTransform));
        safeAreaObject.transform.SetParent(canvas.transform, false);
        RectTransform safeRoot = safeAreaObject.GetComponent<RectTransform>();
        safeRoot.anchorMin = Vector2.zero;
        safeRoot.anchorMax = Vector2.one;
        safeRoot.offsetMin = Vector2.zero;
        safeRoot.offsetMax = Vector2.zero;
        safeAreaObject.AddComponent<MobileSafeAreaFitter>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        CreateHoldButton(safeRoot, "◀", MobileHoldButton.ActionType.SteerLeft,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeRoot, "▶", MobileHoldButton.ActionType.SteerRight,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(154f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeRoot, "▼", MobileHoldButton.ActionType.Brake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-286f, 26f), new Vector2(116f, 92f));
        CreateHoldButton(safeRoot, "▲", MobileHoldButton.ActionType.Throttle,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152f, 26f), new Vector2(128f, 92f));
        CreateHoldButton(safeRoot, "P", MobileHoldButton.ActionType.Handbrake,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152f, 132f), new Vector2(128f, 66f));

        GameObject speedPanel = CreatePanel(safeRoot, "Speed panel", new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(28f, -24f), new Vector2(286f, 62f), new Color(0.025f, 0.055f, 0.09f, 0.9f));
        speedText = CreateText(speedPanel.transform, "VITESSE  0 km/h", 25, TextAnchor.MiddleLeft);
        speedText.rectTransform.offsetMin = new Vector2(16f, 0f);
        speedText.rectTransform.offsetMax = new Vector2(-8f, 0f);

        GameObject titlePanel = CreatePanel(safeRoot, "Game title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -24f), new Vector2(430f, 48f), new Color(0.025f, 0.055f, 0.09f, 0.72f));
        Text title = CreateText(titlePanel.transform, "BUS & COACH SIMULATOR AFRICA", 18, TextAnchor.MiddleCenter);
        title.color = new Color(0.88f, 0.93f, 1f);

        GameObject qualityPanel = CreatePanel(safeRoot, "Graphics preset button", new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-28f, -24f), new Vector2(286f, 62f), new Color(0.025f, 0.055f, 0.09f, 0.9f));
        Button qualityButton = qualityPanel.AddComponent<Button>();
        qualityButton.targetGraphic = qualityPanel.GetComponent<Image>();
        qualityButton.onClick.AddListener(CycleGraphics);
        qualityText = CreateText(qualityPanel.transform, "GRAPHISMES  ÉCONOMIE", 19, TextAnchor.MiddleCenter);

        leftIndicatorImage = CreateUtilityButton(safeRoot, "←", new Vector2(0.5f, 1f), new Vector2(-330f, -88f), () => vehicle.ToggleLeftIndicator());
        hazardImage = CreateUtilityButton(safeRoot, "⚠", new Vector2(0.5f, 1f), new Vector2(-198f, -88f), () => vehicle.ToggleHazardLights());
        rightIndicatorImage = CreateUtilityButton(safeRoot, "→", new Vector2(0.5f, 1f), new Vector2(-66f, -88f), () => vehicle.ToggleRightIndicator());
        headlightImage = CreateUtilityButton(safeRoot, "☼", new Vector2(0.5f, 1f), new Vector2(66f, -88f), () => vehicle.ToggleHeadlights());
        highBeamImage = CreateUtilityButton(safeRoot, "✦", new Vector2(0.5f, 1f), new Vector2(198f, -88f), () => vehicle.ToggleHighBeam());
        CreateUtilityButton(safeRoot, "◎", new Vector2(0.5f, 1f), new Vector2(330f, -88f), ToggleCameraView);
        engineImage = CreateUtilityButton(safeRoot, "⏻", new Vector2(0.5f, 1f), new Vector2(0f, -150f), ToggleEngine);
        CreateSeatbeltButton(safeRoot);
        CreateWiperButton(safeRoot);
        CreatePassengerDoorButton(safeRoot);
        CreateGearSelectorButton(safeRoot);
        CreateCabinLightButton(safeRoot);
        CreateAirConditioningButton(safeRoot);
        CreateRoutePlannerButton(safeRoot);
    }

    private void CreateSeatbeltButton(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "Seatbelt control", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -205f), new Vector2(74f, 48f),
            new Color(0.035f, 0.075f, 0.11f, 0.92f));
        Button button = panel.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(ToggleSeatbelt);
        seatbeltPanelImage = panel.GetComponent<Image>();
        seatbeltIcon = CreateIcon(panel.transform, CoachHudIcon.IconType.SeatbeltOn);
    }


    private void CreatePassengerDoorButton(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "Passenger door control", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -205f), new Vector2(74f, 48f),
            new Color(0.035f, 0.075f, 0.11f, 0.92f));
        Button button = panel.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(TogglePassengerDoor);
        doorPanelImage = panel.GetComponent<Image>();
        doorIcon = CreateIcon(panel.transform, CoachHudIcon.IconType.DoorClosed);
    }


    private void CreateWiperButton(Transform parent)
    {
        wiperImage = CreateUtilityButton(parent, "≋", new Vector2(0.5f, 1f),
            new Vector2(0f, -205f), ToggleWipers);
    }

    private void ToggleWipers()
    {
        if (wipers != null) wipers.Toggle();
    }


    private void CreateGearSelectorButton(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "Gear selector", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -270f), new Vector2(74f, 48f),
            new Color(0.035f, 0.075f, 0.11f, 0.92f));
        Button button = panel.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(CycleGearSelection);
        gearPanelImage = panel.GetComponent<Image>();
        gearText = CreateText(panel.transform, vehicle != null ? vehicle.CurrentGearLabel : "P", 25, TextAnchor.MiddleCenter);
        gearText.resizeTextMinSize = 18;
        gearText.resizeTextMaxSize = 28;
    }


    private void CreateCabinLightButton(Transform parent)
    {
        cabinLightImage = CreateUtilityButton(parent, "☾", new Vector2(0.5f, 1f),
            new Vector2(-130f, -270f), ToggleCabinLights);
    }


    private void CreateRoutePlannerButton(Transform parent)
    {
        CreateUtilityButton(parent, "⌖", new Vector2(0.5f, 1f),
            new Vector2(460f, -88f), () => { if (openRoutePlanner != null) openRoutePlanner(); });
    }

    private void CreateAirConditioningButton(Transform parent)
    {
        acImage = CreateUtilityButton(parent, "❄", new Vector2(0.5f, 1f),
            new Vector2(130f, -270f), ToggleAirConditioning);
    }

    private void ToggleAirConditioning()
    {
        if (cabinComfort != null) cabinComfort.ToggleAirConditioning();
    }

    private void ToggleCabinLights()
    {
        if (cabinLights != null) cabinLights.Toggle();
    }

    private void CycleGearSelection()
    {
        if (vehicle != null && !vehicle.CycleGearSelection())
            Debug.Log("Boîte : sélectionnez P/R/N/D uniquement à l'arrêt.");
    }

    private void TogglePassengerDoor()
    {
        if (vehicle != null && !vehicle.TogglePassengerDoor())
            Debug.Log("Porte passagers verrouillée : arrêtez complètement le car.");
        if (vehicle != null && doorIcon != null)
            doorIcon.SetIcon(vehicle.PassengerDoorOpen ? CoachHudIcon.IconType.DoorOpen : CoachHudIcon.IconType.DoorClosed);
    }

    private void ToggleSeatbelt()
    {
        if (vehicle != null && !vehicle.SetSeatbelt(!vehicle.SeatbeltFastened))
            Debug.Log("Ceinture : détachez-la uniquement lorsque le car est arrêté.");
        if (vehicle != null && seatbeltIcon != null)
            seatbeltIcon.SetIcon(vehicle.SeatbeltFastened ? CoachHudIcon.IconType.SeatbeltOn : CoachHudIcon.IconType.SeatbeltOff);
    }

    private void ToggleEngine()
    {
        if (vehicle != null && !vehicle.ToggleEngine())
            Debug.Log("Démarrage impossible : vérifiez l'arrêt, P/N, la porte, la ceinture et le carburant.");
    }

    private void ToggleCameraView()
    {
        if (cameraView != null) cameraView.CycleView();
    }

    private Image CreateUtilityButton(Transform parent, string label, Vector2 anchor, Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        GameObject panel = CreatePanel(parent, label + " control", anchor, new Vector2(0.5f, 0.5f), position,
            new Vector2(124f, 46f), new Color(0.035f, 0.075f, 0.11f, 0.92f));
        Button button = panel.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(action);
        CreateIcon(panel.transform, IconForSymbol(label));
        return panel.GetComponent<Image>();
    }


    private static CoachHudIcon.IconType IconForSymbol(string symbol)
    {
        switch (symbol)
        {
            case "◀": case "←": return CoachHudIcon.IconType.ArrowLeft;
            case "▶": case "→": return CoachHudIcon.IconType.ArrowRight;
            case "⚠": return CoachHudIcon.IconType.Warning;
            case "☼": return CoachHudIcon.IconType.Headlights;
            case "✦": return CoachHudIcon.IconType.HighBeam;
            case "◎": return CoachHudIcon.IconType.Camera;
            case "⏻": return CoachHudIcon.IconType.Power;
            case "▲": return CoachHudIcon.IconType.PedalUp;
            case "▼": return CoachHudIcon.IconType.PedalDown;
            case "P": return CoachHudIcon.IconType.Parking;
            case "≋": return CoachHudIcon.IconType.Wipers;
            case "☾": return CoachHudIcon.IconType.CabinLight;
            case "❄": return CoachHudIcon.IconType.AirConditioning;
            case "⌖": return CoachHudIcon.IconType.RouteMap;
            default: return CoachHudIcon.IconType.Warning;
        }
    }

    private static CoachHudIcon CreateIcon(Transform parent, CoachHudIcon.IconType iconType)
    {
        GameObject iconObject = new GameObject("Vector icon", typeof(RectTransform), typeof(CoachHudIcon));
        iconObject.transform.SetParent(parent, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.18f, 0.12f);
        rect.anchorMax = new Vector2(0.82f, 0.88f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        CoachHudIcon icon = iconObject.GetComponent<CoachHudIcon>();
        icon.color = Color.white;
        icon.SetIcon(iconType);
        icon.raycastTarget = false;
        return icon;
    }

    private static void SetButtonState(Image image, bool active)
    {
        if (image == null) return;
        image.color = active
            ? new Color(0.10f, 0.42f, 0.30f, 0.98f)
            : new Color(0.035f, 0.075f, 0.11f, 0.92f);
    }

    private void CycleGraphics()
    {
        if (graphics != null) graphics.CyclePreset();
    }

    private void CreateHoldButton(Transform parent, string label, MobileHoldButton.ActionType action,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        Color color = action == MobileHoldButton.ActionType.Throttle
            ? new Color(0.04f, 0.34f, 0.27f, 0.94f)
            : action == MobileHoldButton.ActionType.Brake || action == MobileHoldButton.ActionType.Handbrake
                ? new Color(0.48f, 0.16f, 0.13f, 0.94f)
                : new Color(0.08f, 0.22f, 0.38f, 0.94f);
        GameObject panel = CreatePanel(parent, label + " button", anchor, pivot, position, size, color);
        panel.AddComponent<MobileHoldButton>().Configure(input, action);
        CreateIcon(panel.transform, IconForSymbol(label));
    }

    private static GameObject CreatePanel(Transform parent, string objectName, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return panel;
    }

    private static Text CreateText(Transform parent, string value, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
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
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }
}