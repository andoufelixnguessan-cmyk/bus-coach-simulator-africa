using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Mobile route-selection interface connected to the saved company, coach ownership and trip factory.
/// It prepares a trip session; physical route progression is handled by the driving world.
/// </summary>
public sealed class RoutePlanningPanel : MonoBehaviour
{
    private CompanyProfile company;
    private readonly List<string> ownedCoachIds = new List<string>();
    private readonly List<string> selectedCityIds = new List<string>();
    private Action<PlayerRouteTripSetup> onTripPrepared;
    private Action onClosed;

    private Dropdown cityDropdown;
    private Dropdown coachDropdown;
    private Dropdown serviceDropdown;
    private Text routeText;
    private Text feedbackText;
    private Button addCityButton;
    private Button prepareButton;
    private Font runtimeFont;

    public void Configure(CompanyProfile playerCompany, IList<string> playerOwnedCoachIds,
        Action<PlayerRouteTripSetup> tripPrepared, Action panelClosed = null)
    {
        company = playerCompany;
        ownedCoachIds.Clear();
        if (playerOwnedCoachIds != null)
        {
            for (int i = 0; i < playerOwnedCoachIds.Count; i++)
            {
                string id = playerOwnedCoachIds[i];
                CoachDefinition definition = CoachEconomy.FindCoach(id);
                if (!string.IsNullOrWhiteSpace(id) && definition != null && definition.MinimumWheelCount >= 6 &&
                    !ownedCoachIds.Contains(id))
                    ownedCoachIds.Add(id);
            }
        }
        onTripPrepared = tripPrepared;
        onClosed = panelClosed;
        RefreshCoachOptions();
        RefreshRouteText();
        SetFeedback(company == null ? "Entreprise introuvable." :
            ownedCoachIds.Count == 0 ? "Aucun car compatible de 6 roues ou plus dans votre garage." :
            "Choisissez votre ville de départ, puis ajoutez les étapes de votre trajet.", false);
    }

    private void Awake()
    {
        runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (runtimeFont == null) runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        BuildInterface();
    }

    private void BuildInterface()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("RoutePlanningPanel doit être placé sous un Canvas.");
            enabled = false;
            return;
        }

        GameObject panel = CreateUIObject("Route planning panel", transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.025f, 0.055f, 0.105f, 0.985f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.04f, 0.04f);
        panelRect.anchorMax = new Vector2(0.96f, 0.96f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.AddComponent<MobileSafeAreaFitter>();
        panelRect.offsetMin = new Vector2(10f, 10f);
        panelRect.offsetMax = new Vector2(-10f, -10f);

        // The form is vertically scrollable so every control remains reachable on small phones.
        panel.AddComponent<RectMask2D>();
        ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
        panelScroll.horizontal = false;
        panelScroll.vertical = true;
        panelScroll.movementType = ScrollRect.MovementType.Clamped;
        panelScroll.scrollSensitivity = 34f;
        panelScroll.inertia = true;

        GameObject contentObject = CreateUIObject("Scrollable content", panel.transform);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 900f);
        panelScroll.viewport = panelRect;
        panelScroll.content = contentRect;

        AddText(contentObject.transform, "BUS & COACH SIMULATOR AFRICA", 13, 0, -8, 28, TextAnchor.MiddleCenter, new Color(0.48f, 0.75f, 0.98f));
        AddText(contentObject.transform, "PLANIFIER UN VOYAGE", 25, 0, -30, 54, TextAnchor.MiddleCenter, Color.white);
        AddText(contentObject.transform, "Réseau africain • Cars longue distance uniquement", 13, 0, -72, 38,
            TextAnchor.MiddleCenter, new Color(0.68f, 0.82f, 0.95f));

        AddText(contentObject.transform, "VOTRE ITINÉRAIRE", 14, 0, -112, 38,
            TextAnchor.MiddleLeft, Color.white);
        cityDropdown = AddDropdown(contentObject.transform, "Choisir une ville", 0, -148, 52);

        addCityButton = AddButton(contentObject.transform, "+ AJOUTER CETTE VILLE", 0, -208, 52, AddSelectedCity);
        AddText(contentObject.transform, "VOTRE CAR", 14, 0, -270, 38, TextAnchor.MiddleLeft, Color.white);
        coachDropdown = AddDropdown(contentObject.transform, "Choisir un car", 0, -306, 52);

        AddText(contentObject.transform, "CLASSE DE SERVICE", 14, 0, -368, 38, TextAnchor.MiddleLeft, Color.white);
        serviceDropdown = AddDropdown(contentObject.transform, "Économique", 0, -404, 52);
        serviceDropdown.options = new List<Dropdown.OptionData> {
            new Dropdown.OptionData("Économique"),
            new Dropdown.OptionData("Confort"),
            new Dropdown.OptionData("VIP")
        };
        serviceDropdown.RefreshShownValue();

        routeText = AddText(contentObject.transform, "", 15, 0, -478, 132, TextAnchor.UpperLeft, Color.white);
        AddButton(contentObject.transform, "TRAJET CONSEILLÉ : ABIDJAN → YAMOUSSOUKRO", 0, -622, 50, UseRecommendedRoute);
        AddButton(contentObject.transform, "ANNULER LA DERNIÈRE VILLE", 0, -680, 48, RemoveLastCity);
        AddButton(contentObject.transform, "RÉINITIALISER LE TRAJET", 0, -736, 48, ResetRoute);
        prepareButton = AddButton(contentObject.transform, "PRÉPARER LE VOYAGE", 0, -792, 56, PrepareTrip);
        feedbackText = AddText(contentObject.transform, "", 14, 0, -856, 64, TextAnchor.MiddleCenter,
            new Color(1f, 0.78f, 0.35f));
        AddButton(contentObject.transform, "FERMER ET CONDUIRE", 0, -926, 52, ClosePanel);
        contentRect.sizeDelta = new Vector2(0f, 1020f);
        RefreshCityOptions();
    }

    private void RefreshCityOptions()
    {
        if (cityDropdown == null) return;
        cityDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        string currentCityId = selectedCityIds.Count == 0 ? null : selectedCityIds[selectedCityIds.Count - 1];
        List<AfricanRoutePlanner.City> cities = currentCityId == null
            ? new List<AfricanRoutePlanner.City>(AfricanRoutePlanner.AllCities)
            : AfricanRoutePlanner.GetDirectConnections(currentCityId);
        for (int i = 0; i < cities.Count; i++)
            options.Add(new Dropdown.OptionData(cities[i].Name + " (" + cities[i].CountryCode + ")"));
        cityDropdown.AddOptions(options);
        cityDropdown.interactable = options.Count > 0;
        if (options.Count == 0) SetFeedback("Aucune liaison disponible depuis cette ville dans le réseau actuel.", true);
    }

    private void RefreshCoachOptions()
    {
        if (coachDropdown == null) return;
        coachDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < ownedCoachIds.Count; i++)
        {
            CoachDefinition coach = CoachEconomy.FindCoach(ownedCoachIds[i]);
            if (coach != null) options.Add(new Dropdown.OptionData(coach.DisplayName + " • " + coach.MinimumWheelCount + " roues"));
        }
        coachDropdown.AddOptions(options);
        coachDropdown.interactable = options.Count > 0;
    }

    private void AddSelectedCity()
    {
        if (cityDropdown == null || cityDropdown.options.Count == 0) return;
        string currentId = selectedCityIds.Count == 0 ? null : selectedCityIds[selectedCityIds.Count - 1];
        List<AfricanRoutePlanner.City> choices = currentId == null
            ? new List<AfricanRoutePlanner.City>(AfricanRoutePlanner.AllCities)
            : AfricanRoutePlanner.GetDirectConnections(currentId);
        int index = Mathf.Clamp(cityDropdown.value, 0, choices.Count - 1);
        AfricanRoutePlanner.City city = choices[index];
        if (selectedCityIds.Contains(city.Id))
        {
            SetFeedback("Cette ville figure déjà dans votre trajet.", true);
            return;
        }
        selectedCityIds.Add(city.Id);
        RefreshCityOptions();
        RefreshRouteText();
        SetFeedback(selectedCityIds.Count < 2 ? "Départ enregistré. Ajoutez maintenant une destination." :
            "Étape ajoutée. Vous pouvez continuer ou préparer le voyage.", false);
    }

    private void UseRecommendedRoute()
    {
        selectedCityIds.Clear();
        selectedCityIds.Add("abidjan");
        selectedCityIds.Add("yamoussoukro");
        RefreshCityOptions();
        RefreshRouteText();
        SetFeedback("Trajet conseillé sélectionné. Vérifiez votre car et préparez le voyage.", false);
    }

    private void RemoveLastCity()
    {
        if (selectedCityIds.Count == 0)
        {
            SetFeedback("Aucune ville à retirer.", true);
            return;
        }

        selectedCityIds.RemoveAt(selectedCityIds.Count - 1);
        RefreshCityOptions();
        RefreshRouteText();
        SetFeedback(selectedCityIds.Count == 0
            ? "Itinéraire vidé. Choisissez une ville de départ."
            : "Dernière ville retirée. Vous pouvez choisir une autre étape.", false);
    }

    private void ResetRoute()
    {
        selectedCityIds.Clear();
        RefreshCityOptions();
        RefreshRouteText();
        SetFeedback("Trajet réinitialisé. Choisissez une ville de départ.", false);
    }

    private void ClosePanel()
    {
        if (onClosed != null) onClosed();
        else gameObject.SetActive(false);
    }

    private void PrepareTrip()
    {
        if (company == null || ownedCoachIds.Count == 0)
        {
            SetFeedback("Configurez d'abord l'entreprise et un car compatible du garage.", true);
            return;
        }
        if (selectedCityIds.Count < 2)
        {
            SetFeedback("Ajoutez au moins une ville de départ et une destination.", true);
            return;
        }

        string coachId = ownedCoachIds[Mathf.Clamp(coachDropdown.value, 0, ownedCoachIds.Count - 1)];
        CoachServiceClass service = (CoachServiceClass)Mathf.Clamp(serviceDropdown.value, 0, 2);
        PlayerRouteTripSetup setup;
        string reason;
        string tripId = Guid.NewGuid().ToString("N");
        if (!PlayerRouteTripFactory.TryCreate(tripId, selectedCityIds, company, coachId, service, out setup, out reason))
        {
            SetFeedback(reason, true);
            return;
        }

        long fare = setup.GetPassengerFareXof();
        long estimatedBorderFees = setup.AnnouncedBorderFeesXof;
        string summary = setup.Itinerary.DisplayName + "\n" +
            setup.Itinerary.EstimatedDistanceKm.ToString("N0") + " km • " +
            FormatDuration(setup.Itinerary.EstimatedTotalMinutes) + "\n" +
            "Billet : " + fare.ToString("N0") + " FCFA / passager\n" +
            "Frais frontaliers annoncés : " + estimatedBorderFees.ToString("N0") + " FCFA";
        SetFeedback("Voyage préparé.\n" + summary, false);
        if (onTripPrepared != null) onTripPrepared(setup);
    }

    private void RefreshRouteText()
    {
        if (routeText == null) return;
        if (selectedCityIds.Count == 0)
        {
            routeText.text = "VOTRE ITINÉRAIRE\nAucune ville sélectionnée.";
            return;
        }
        List<string> names = new List<string>();
        int km = 0;
        RouteItinerary preview;
        string reason;
        for (int i = 0; i < selectedCityIds.Count; i++)
        {
            AfricanRoutePlanner.City city = AfricanRoutePlanner.FindCity(selectedCityIds[i]);
            if (city != null) names.Add(city.Name);
        }
        routeText.text = "VOTRE ITINÉRAIRE\n" + string.Join("  →  ", names.ToArray());
        if (selectedCityIds.Count >= 2 &&
            AfricanRoutePlanner.TryBuildPlayerItinerary(selectedCityIds, out preview, out reason))
        {
            km = preview.EstimatedDistanceKm;
            RouteFare fare = RouteEconomy.CreateFareForItinerary(preview);
            long borderFees = 0L;
            for (int i = 0; i < preview.Borders.Count; i++) borderFees += preview.Borders[i].CoachAndCompanyFeeXof;
            routeText.text += "\nDistance estimée : " + km.ToString("N0") + " km" +
                "\nDurée estimée : " + FormatDuration(preview.EstimatedTotalMinutes) +
                "\nFrais frontière : " + borderFees.ToString("N0") + " FCFA" +
                (fare == null ? "" : "\nTarif éco indicatif : " + fare.EconomyXof.ToString("N0") + " FCFA / passager");
        }
    }

    private void SetFeedback(string message, bool isError)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.color = isError ? new Color(1f, 0.48f, 0.42f) : new Color(0.58f, 0.92f, 0.7f);
    }

    private static string FormatDuration(int minutes)
    {
        return (minutes / 60).ToString() + " h " + (minutes % 60).ToString("00") + " min";
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private Text AddText(Transform parent, string value, int size, float x, float y, float height,
        TextAnchor alignment, Color color)
    {
        GameObject go = CreateUIObject("Text", parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.06f, 1f);
        rect.anchorMax = new Vector2(0.94f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Text text = go.AddComponent<Text>();
        text.font = runtimeFont;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 11;
        text.resizeTextMaxSize = size;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = value;
        return text;
    }

    private Dropdown AddDropdown(Transform parent, string placeholder, float x, float y, float height)
    {
        GameObject go = CreateUIObject("Dropdown", parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.06f, 1f);
        rect.anchorMax = new Vector2(0.94f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.09f, 0.16f, 0.25f, 1f);
        Dropdown dropdown = go.AddComponent<Dropdown>();
        dropdown.targetGraphic = image;
        dropdown.options = new List<Dropdown.OptionData> { new Dropdown.OptionData(placeholder) };
        Text caption = AddText(go.transform, placeholder, 16, 0, 0, height, TextAnchor.MiddleCenter, Color.white);
        dropdown.captionText = caption;
        GameObject template = CreateUIObject("Template", go.transform);
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = Vector2.zero;
        templateRect.sizeDelta = new Vector2(0f, 180f);
        Image templateImage = template.AddComponent<Image>();
        templateImage.color = new Color(0.08f, 0.13f, 0.2f);
        ScrollRect scroll = template.AddComponent<ScrollRect>();
        GameObject viewport = CreateUIObject("Viewport", template.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white;
        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 36f);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GameObject item = CreateUIObject("Item", content.transform);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0f, 40f);
        Toggle toggle = item.AddComponent<Toggle>();
        Image itemImage = item.AddComponent<Image>();
        itemImage.color = new Color(0.12f, 0.2f, 0.3f);
        Text itemText = AddText(item.transform, "Option", 15, 0, 0, 40, TextAnchor.MiddleLeft, Color.white);
        itemText.rectTransform.offsetMin = new Vector2(12f, 0f);
        toggle.targetGraphic = itemImage;
        toggle.graphic = itemText;
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        dropdown.template = templateRect;
        dropdown.itemText = itemText;
        template.SetActive(false);
        return dropdown;
    }

    private Button AddButton(Transform parent, string label, float x, float y, float height, UnityAction action)
    {
        GameObject go = CreateUIObject(label, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.06f, 1f);
        rect.anchorMax = new Vector2(0.94f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Image image = go.AddComponent<Image>();
        bool primary = label.Contains("PRÉPARER");
        bool recommended = label.Contains("CONSEILLÉ");
        image.color = primary ? new Color(0.88f, 0.57f, 0.16f) :
            recommended ? new Color(0.08f, 0.43f, 0.36f) : new Color(0.08f, 0.32f, 0.55f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = Color.Lerp(image.color, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(image.color, Color.black, 0.18f);
        colors.selectedColor = image.color;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(action);
        Text text = AddText(go.transform, label, 16, 0, 0, height, TextAnchor.MiddleCenter, Color.white);
        text.fontStyle = FontStyle.Bold;
        return button;
    }
}
