using UnityEngine;
using UnityEngine.UI;

/// <summary>Landscape instrument cluster and condition lamps for the driving HUD prototype.</summary>
public sealed class CoachDashboardHUD : MonoBehaviour
{
    private CoachVehicleController vehicle;
    private Text speed, rpm, fuel, temperature, oil, air, battery, model;
    private Text leftLamp, rightLamp, highBeam, parking, seatbelt, abs, engine;
    private int refreshFrame;

    public void Configure(CoachVehicleController target, CoachDashboardProfile profile, Transform parent)
    {
        vehicle = target;
        GameObject cluster = Panel(parent, "Instrument cluster", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 18f), new Vector2(760f, 142f), new Color(0.012f, 0.025f, 0.045f, 0.94f));
        model = Label(cluster.transform, profile.DisplayName.ToUpperInvariant() + "  •  " + profile.EngineFamily, 14, TextAnchor.MiddleCenter);
        model.rectTransform.anchorMin = new Vector2(0f, 1f);
        model.rectTransform.anchorMax = new Vector2(1f, 1f);
        model.rectTransform.pivot = new Vector2(0.5f, 1f);
        model.rectTransform.anchoredPosition = new Vector2(0f, -4f);
        model.rectTransform.sizeDelta = new Vector2(0f, 25f);

        speed = MakeReadout(cluster.transform, "0", new Vector2(0.03f, 0.13f), new Vector2(0.22f, 0.78f), 42);
        rpm = MakeReadout(cluster.transform, "650 tr/min", new Vector2(0.24f, 0.13f), new Vector2(0.43f, 0.78f), 22);
        fuel = MakeReadout(cluster.transform, "CARBURANT 86%", new Vector2(0.45f, 0.13f), new Vector2(0.62f, 0.78f), 17);
        temperature = MakeReadout(cluster.transform, "EAU 82°C", new Vector2(0.63f, 0.13f), new Vector2(0.76f, 0.78f), 16);
        oil = MakeReadout(cluster.transform, "HUILE 4.2 bar", new Vector2(0.77f, 0.13f), new Vector2(0.9f, 0.78f), 15);
        air = MakeReadout(cluster.transform, "AIR 8.2 bar", new Vector2(0.90f, 0.13f), new Vector2(0.99f, 0.78f), 14);
        battery = MakeReadout(cluster.transform, "24V", new Vector2(0.02f, 0.02f), new Vector2(0.10f, 0.22f), 12);

        leftLamp = Lamp(cluster.transform, "◀ CLIGNO", new Vector2(0.12f, 0.02f), new Vector2(0.24f, 0.22f));
        rightLamp = Lamp(cluster.transform, "CLIGNO ▶", new Vector2(0.25f, 0.02f), new Vector2(0.37f, 0.22f));
        highBeam = Lamp(cluster.transform, "PLEINS PHARES", new Vector2(0.38f, 0.02f), new Vector2(0.54f, 0.22f));
        parking = Lamp(cluster.transform, "FREIN PARK", new Vector2(0.55f, 0.02f), new Vector2(0.68f, 0.22f));
        seatbelt = Lamp(cluster.transform, "CEINTURE", new Vector2(0.69f, 0.02f), new Vector2(0.80f, 0.22f));
        abs = Lamp(cluster.transform, "ABS", new Vector2(0.81f, 0.02f), new Vector2(0.89f, 0.22f));
        engine = Lamp(cluster.transform, "MOTEUR", new Vector2(0.90f, 0.02f), new Vector2(0.99f, 0.22f));
    }

    private void Update()
    {
        if (vehicle == null || Time.frameCount - refreshFrame < 5) return;
        refreshFrame = Time.frameCount;
        speed.text = Mathf.RoundToInt(vehicle.SpeedKph) + "\nkm/h";
        rpm.text = "COMPTE-TOURS\n" + Mathf.RoundToInt(vehicle.EngineRpm) + " tr/min";
        fuel.text = "CARBURANT\n" + Mathf.RoundToInt(vehicle.FuelPercent) + "%";
        temperature.text = "TEMP. EAU\n" + Mathf.RoundToInt(vehicle.CoolantTemperatureC) + "°C";
        oil.text = "PRESSION HUILE\n" + vehicle.OilPressureBar.ToString("0.0") + " bar";
        air.text = "PRESSION AIR\n" + vehicle.AirPressureBar.ToString("0.0") + " bar";
        battery.text = vehicle.BatteryVoltage.ToString("0.0") + " V";
        SetLamp(leftLamp, vehicle.LeftIndicatorOn, new Color(0.25f, 0.95f, 0.28f));
        SetLamp(rightLamp, vehicle.RightIndicatorOn, new Color(0.25f, 0.95f, 0.28f));
        SetLamp(highBeam, vehicle.HighBeamOn, new Color(0.2f, 0.55f, 1f));
        SetLamp(parking, vehicle.ParkingBrakeApplied, new Color(1f, 0.35f, 0.12f));
        SetLamp(seatbelt, !vehicle.SeatbeltFastened, new Color(1f, 0.18f, 0.12f));
        SetLamp(abs, vehicle.AbsWarning, new Color(1f, 0.55f, 0.08f));
        SetLamp(engine, vehicle.EngineWarning, new Color(1f, 0.12f, 0.08f));
    }

    private static void SetLamp(Text lamp, bool active, Color on)
    {
        lamp.color = active ? on : new Color(0.30f, 0.36f, 0.42f, 0.55f);
    }

    private static Text MakeReadout(Transform parent, string value, Vector2 min, Vector2 max, int size)
    {
        Text t = Label(parent, value, size, TextAnchor.MiddleCenter);
        t.rectTransform.anchorMin = min; t.rectTransform.anchorMax = max;
        t.rectTransform.offsetMin = Vector2.zero; t.rectTransform.offsetMax = Vector2.zero;
        return t;
    }

    private static Text Lamp(Transform parent, string value, Vector2 min, Vector2 max)
    {
        Text t = MakeReadout(parent, value, min, max, 11);
        t.color = new Color(0.30f, 0.36f, 0.42f, 0.55f);
        return t;
    }

    private static GameObject Panel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = pivot; r.anchoredPosition = pos; r.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static Text Label(Transform parent, string value, int fontSize, TextAnchor alignment)
    {
        GameObject go = new GameObject("Instrument", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = value; t.fontSize = fontSize; t.fontStyle = FontStyle.Bold;
        t.alignment = alignment; t.color = new Color(0.88f, 0.94f, 1f);
        t.resizeTextForBestFit = true; t.resizeTextMinSize = 10; t.resizeTextMaxSize = fontSize;
        t.raycastTarget = false;
        RectTransform r = t.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        return t;
    }
}
