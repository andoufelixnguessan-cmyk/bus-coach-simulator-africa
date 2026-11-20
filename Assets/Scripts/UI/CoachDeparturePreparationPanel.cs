using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pre-departure safety checklist, operating-cost estimate and controlled departure.</summary>
public sealed class CoachDeparturePreparationPanel : MonoBehaviour
{
    private PlayerTripSession session;
    private CompanyProfile company;
    private Action openManifest;
    private Action onDeparted;
    private readonly List<Toggle> checks = new List<Toggle>();
    private Text statusText;
    private Text costText;
    private Text balanceText;
    private Font font;
    private TripOperatingCosts costs;

    private static readonly string[] CheckLabels =
    {
        "Pneus et écrous de roues contrôlés",
        "Freins et frein de stationnement vérifiés",
        "Feux et clignotants fonctionnels",
        "Rétroviseurs et pare-brise contrôlés",
        "Portes et issues de secours vérifiées",
        "Ceintures de sécurité disponibles",
        "Extincteur présent à bord",
        "Trousse de premiers secours présente",
        "Bagages correctement rangés et sécurisés",
        "Conducteur reposé et apte au trajet"
    };

    public void Configure(PlayerTripSession tripSession, CompanyProfile owner,
        Action showManifest, Action departed)
    {
        session = tripSession;
        company = owner;
        openManifest = showManifest;
        onDeparted = departed;
        costs = BuildOperatingCostEstimate();
        RefreshFinancialLabels();
    }

    private void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        BuildInterface();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        RefreshFinancialLabels();
        SetStatus("Cochez les 10 contrôles, puis vérifiez le manifeste passagers.", false);
    }

    public void HideForManifest()
    {
        gameObject.SetActive(false);
        if (openManifest != null) openManifest();
    }

    private void BuildInterface()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) { enabled = false; return; }

        GameObject panel = Make("Departure preparation background", transform);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.025f, 0.055f, 0.105f, 0.99f);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.04f, 0.035f);
        pr.anchorMax = new Vector2(0.96f, 0.965f);
        pr.offsetMin = Vector2.zero;
        pr.offsetMax = Vector2.zero;
        panel.AddComponent<MobileSafeAreaFitter>();
        ScrollRect scroll = panel.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 34f;

        GameObject body = Make("Departure preparation content", panel.transform);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 1f);
        br.anchorMax = new Vector2(1f, 1f);
        br.pivot = new Vector2(0.5f, 1f);
        br.sizeDelta = new Vector2(0f, 1050f);
        scroll.viewport = pr;
        scroll.content = br;

        Transform t = body.transform;
        AddText(t, "PRÉPARATION DU DÉPART", 24, -8, 48, TextAnchor.MiddleCenter, Color.white);
        AddText(t, "CONTRÔLES DE SÉCURITÉ OBLIGATOIRES", 17, -62, 34, TextAnchor.MiddleLeft, Color.white);
        float y = -105f;
        for (int i = 0; i < CheckLabels.Length; i++)
        {
            Toggle toggle = AddCheck(t, CheckLabels[i], y);
            checks.Add(toggle);
            y -= 48f;
        }

        AddText(t, "BUDGET PRÉVISIONNEL DU VOYAGE", 17, -600, 34, TextAnchor.MiddleLeft, Color.white);
        costText = AddText(t, "", 15, -638, 180, TextAnchor.UpperLeft, new Color(0.86f, 0.91f, 0.97f));
        balanceText = AddText(t, "", 16, -820, 34, TextAnchor.MiddleCenter, new Color(0.62f, 0.84f, 1f));
        AddButton(t, "OUVRIR LE MANIFESTE PASSAGERS", -870, 48, HideForManifest);
        AddButton(t, "VALIDER LES CONTRÔLES ET DÉMARRER LE VOYAGE", -926, 58, Depart);
        statusText = AddText(t, "", 14, -990, 44, TextAnchor.MiddleCenter, new Color(0.58f, 0.92f, 0.7f));
    }

    private TripOperatingCosts BuildOperatingCostEstimate()
    {
        long km = session != null && session.Setup != null && session.Setup.Itinerary != null
            ? Math.Max(1L, session.Setup.Itinerary.EstimatedDistanceKm) : 1L;
        return new TripOperatingCosts
        {
            FuelXof = km * 150L,
            DriverAndStaffXof = 45000L + km * 25L,
            TollsAndBorderXof = session != null && session.Setup != null
                ? session.Setup.AnnouncedBorderFeesXof : 0L,
            FoodAndDrinksXof = km * 20L,
            MaintenanceReserveXof = km * 35L
        };
    }

    private CoachSafetyChecklist BuildChecklist()
    {
        return new CoachSafetyChecklist
        {
            TiresAndWheelNutsChecked = IsChecked(0),
            BrakesChecked = IsChecked(1),
            LightsAndIndicatorsChecked = IsChecked(2),
            MirrorsAndWindscreenChecked = IsChecked(3),
            DoorsAndEmergencyExitsChecked = IsChecked(4),
            SeatbeltsChecked = IsChecked(5),
            FireExtinguisherPresent = IsChecked(6),
            FirstAidKitPresent = IsChecked(7),
            LuggageSecured = IsChecked(8),
            DriverRested = IsChecked(9)
        };
    }

    private bool IsChecked(int index) { return index < checks.Count && checks[index] != null && checks[index].isOn; }

    private void Depart()
    {
        if (session == null || costs == null)
        { SetStatus("Aucun voyage valide n'est chargé.", true); return; }
        string reason;
        if (!session.TryPrepareAndDepart(BuildChecklist(), costs, out reason))
        {
            SetStatus(reason, true);
            RefreshFinancialLabels();
            return;
        }

        SetStatus("Départ confirmé. Bonne route !", false);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        if (onDeparted != null) onDeparted();
    }

    private void RefreshFinancialLabels()
    {
        if (costText != null && costs != null)
        {
            costText.text = "Carburant : " + costs.FuelXof.ToString("N0") + " FCFA\n" +
                "Conducteur et personnel : " + costs.DriverAndStaffXof.ToString("N0") + " FCFA\n" +
                "Péages et frontières : " + costs.TollsAndBorderXof.ToString("N0") + " FCFA\n" +
                "Restauration : " + costs.FoodAndDrinksXof.ToString("N0") + " FCFA\n" +
                "Réserve entretien : " + costs.MaintenanceReserveXof.ToString("N0") + " FCFA\n\n" +
                "TOTAL À DÉBITER : " + costs.TotalXof.ToString("N0") + " FCFA";
        }
        if (balanceText != null && company != null)
            balanceText.text = "Trésorerie : " + company.BalanceXof.ToString("N0") + " FCFA";
    }

    private void SetStatus(string message, bool error)
    {
        if (statusText == null) return;
        statusText.text = message;
        statusText.color = error ? new Color(1f, 0.48f, 0.42f) : new Color(0.58f, 0.92f, 0.7f);
    }

    private Toggle AddCheck(Transform parent, string label, float y)
    {
        GameObject root = Make("Safety check", parent);
        RectTransform r = root.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.06f, 1f); r.anchorMax = new Vector2(0.94f, 1f);
        r.pivot = new Vector2(0.5f, 1f); r.anchoredPosition = new Vector2(0f, y);
        r.sizeDelta = new Vector2(0f, 42f);
        Toggle toggle = root.AddComponent<Toggle>();
        Image bg = root.AddComponent<Image>(); bg.color = new Color(0.08f, 0.13f, 0.20f, 1f);
        toggle.targetGraphic = bg;
        GameObject mark = Make("Check mark", root.transform);
        RectTransform mr = mark.GetComponent<RectTransform>();
        mr.anchorMin = new Vector2(0f, 0.15f); mr.anchorMax = new Vector2(0f, 0.85f);
        mr.pivot = new Vector2(0f, 0.5f); mr.anchoredPosition = new Vector2(8f, 0f);
        mr.sizeDelta = new Vector2(28f, 0f);
        Image markImage = mark.AddComponent<Image>(); markImage.color = new Color(0.25f, 0.75f, 0.48f);
        toggle.graphic = markImage;
        Text txt = AddText(root.transform, label, 14, 0f, 0f, TextAnchor.MiddleLeft, Color.white);
        txt.rectTransform.anchorMin = new Vector2(0f, 0f); txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = new Vector2(46f, 0f); txt.rectTransform.offsetMax = new Vector2(-6f, 0f);
        return toggle;
    }

    private void AddButton(Transform parent, string label, float y, float height, UnityEngine.Events.UnityAction action)
    {
        GameObject root = Make(label, parent);
        RectTransform r = root.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.06f, 1f); r.anchorMax = new Vector2(0.94f, 1f);
        r.pivot = new Vector2(0.5f, 1f); r.anchoredPosition = new Vector2(0f, y);
        r.sizeDelta = new Vector2(0f, height);
        Image image = root.AddComponent<Image>(); image.color = new Color(0.12f, 0.35f, 0.56f);
        Button button = root.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
        Text text = AddText(root.transform, label, 15, 0f, 0f, TextAnchor.MiddleCenter, Color.white);
        text.rectTransform.offsetMin = new Vector2(6f, 0f); text.rectTransform.offsetMax = new Vector2(-6f, 0f);
    }

    private Text AddText(Transform parent, string value, int size, float y, float height, TextAnchor align, Color color)
    {
        GameObject root = Make("Label", parent);
        RectTransform r = root.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.06f, 1f); r.anchorMax = new Vector2(0.94f, 1f);
        r.pivot = new Vector2(0.5f, 1f); r.anchoredPosition = new Vector2(0f, y);
        r.sizeDelta = new Vector2(0f, height);
        Text txt = root.AddComponent<Text>(); txt.font = font; txt.text = value; txt.fontSize = size;
        txt.fontStyle = FontStyle.Bold; txt.color = color; txt.alignment = align;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap; txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;
        return txt;
    }

    private GameObject Make(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
