using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>In-game passenger reservations, seat assignment and stop-by-stop boarding manifest.</summary>
public sealed class CoachPassengerManifestPanel : MonoBehaviour
{
    private PassengerTrip trip;
    private CompanyProfile company;
    private Action onClosed;
    private readonly List<RouteStop> passengerStops = new List<RouteStop>();
    private InputField passengerNameInput;
    private InputField luggageWeightInput;
    private Dropdown originDropdown;
    private Dropdown destinationDropdown;
    private Dropdown currentStopDropdown;
    private Text summaryText;
    private Text feedbackText;
    private Text bookingListText;
    private Font font;

    public void Configure(PassengerTrip passengerTrip, CompanyProfile owner, Action closeAction)
    {
        trip = passengerTrip;
        company = owner;
        onClosed = closeAction;
        BuildStopOptions();
        RefreshManifest();
        SetFeedback(trip == null ? "Aucun voyage actif." :
            passengerStops.Count < 2 ? "Ce trajet ne contient pas assez d'arrêts passagers." :
            "Réservez les voyageurs puis traitez chaque arrêt dans l'ordre.", false);
    }

    private void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        BuildInterface();
    }

    private void BuildInterface()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("CoachPassengerManifestPanel doit être placé sous un Canvas.");
            enabled = false;
            return;
        }

        GameObject panel = CreateObject("Passenger manifest background", transform);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.025f, 0.055f, 0.105f, 0.985f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.04f);
        panelRect.anchorMax = new Vector2(0.95f, 0.96f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.AddComponent<MobileSafeAreaFitter>();

        AddText(panel.transform, "GESTION DES PASSAGERS", 24, 0, -10, 48, TextAnchor.MiddleCenter, Color.white);
        summaryText = AddText(panel.transform, "", 16, 0, -62, 72, TextAnchor.MiddleCenter,
            new Color(0.62f, 0.84f, 1f));

        AddText(panel.transform, "NOUVELLE RÉSERVATION", 17, 0, -142, 34, TextAnchor.MiddleLeft, Color.white);
        passengerNameInput = AddInput(panel.transform, "Nom du passager", 0, -188, 50);
        luggageWeightInput = AddInput(panel.transform, "Poids du bagage (kg)", 0, -246, 50);
        originDropdown = AddDropdown(panel.transform, "Départ", 0, -304, 48);
        destinationDropdown = AddDropdown(panel.transform, "Destination", 0, -360, 48);
        AddButton(panel.transform, "RÉSERVER ET ENCAISSER LE BILLET", 0, -418, 52, BookPassenger);

        AddText(panel.transform, "EMBARQUEMENT / DESCENTE", 17, 0, -488, 36, TextAnchor.MiddleLeft, Color.white);
        currentStopDropdown = AddDropdown(panel.transform, "Arrêt à traiter", 0, -532, 48);
        AddButton(panel.transform, "TRAITER CET ARRÊT", 0, -588, 52, ProcessCurrentStop);

        AddText(panel.transform, "MANIFESTE DES BILLETS", 16, 0, -656, 34, TextAnchor.MiddleLeft, Color.white);
        bookingListText = AddText(panel.transform, "Aucune réservation.", 14, 0, -694, 150,
            TextAnchor.UpperLeft, new Color(0.88f, 0.91f, 0.95f));
        feedbackText = AddText(panel.transform, "", 14, 0, -850, 54, TextAnchor.MiddleCenter,
            new Color(0.62f, 0.92f, 0.72f));
        AddButton(panel.transform, "FERMER", 0, -910, 48, ClosePanel);
    }

    private void BuildStopOptions()
    {
        passengerStops.Clear();
        if (trip == null || trip.ItineraryStops == null) return;
        for (int i = 0; i < trip.ItineraryStops.Count; i++)
            if (trip.ItineraryStops[i].PassengersMayBoardOrAlight)
                passengerStops.Add(trip.ItineraryStops[i]);

        List<string> names = new List<string>();
        for (int i = 0; i < passengerStops.Count; i++)
            names.Add(passengerStops[i].Name + " (" + passengerStops[i].CountryCode + ")");
        SetOptions(originDropdown, names);
        SetOptions(destinationDropdown, names);
        SetOptions(currentStopDropdown, names);
        if (destinationDropdown != null && names.Count > 1)
            destinationDropdown.value = names.Count - 1;
    }

    private void BookPassenger()
    {
        if (trip == null || company == null || passengerStops.Count < 2)
        {
            SetFeedback("Préparez d'abord un voyage valide.", true);
            return;
        }
        string name = passengerNameInput != null ? passengerNameInput.text.Trim() : string.Empty;
        float luggage = 0f;
        if (luggageWeightInput != null && !string.IsNullOrWhiteSpace(luggageWeightInput.text) &&
            !float.TryParse(luggageWeightInput.text.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out luggage))
        {
            SetFeedback("Saisissez un poids de bagage valide.", true);
            return;
        }
        int originIndex = Mathf.Clamp(originDropdown.value, 0, passengerStops.Count - 1);
        int destinationIndex = Mathf.Clamp(destinationDropdown.value, 0, passengerStops.Count - 1);
        PassengerBooking booking;
        string reason;
        bool booked = trip.TryBookPassenger(name, company, luggage,
            passengerStops[originIndex].Id, passengerStops[destinationIndex].Id,
            out booking, out reason);
        SetFeedback(booked ? "Billet " + booking.TicketId + " enregistré : " +
            booking.FarePaidXof.ToString("N0") + " FCFA." : reason, !booked);
        if (booked && passengerNameInput != null) passengerNameInput.text = string.Empty;
        RefreshManifest();
    }

    private void ProcessCurrentStop()
    {
        if (trip == null || passengerStops.Count == 0) return;
        int index = Mathf.Clamp(currentStopDropdown.value, 0, passengerStops.Count - 1);
        int alighted, boarded;
        string reason;
        bool processed = trip.TryProcessPassengerStop(passengerStops[index].Id, out alighted, out boarded, out reason);
        SetFeedback(processed ? "Arrivées : " + alighted + " • Embarqués : " + boarded : reason, !processed);
        RefreshManifest();
    }

    private void RefreshManifest()
    {
        if (trip == null) return;
        PassengerTripSummary summary = trip.GetSummary();
        if (summaryText != null)
            summaryText.text = "Places : " + summary.AvailableSeats + " / " + summary.TotalSeats +
                "   •   Réservés : " + summary.ReservedPassengers +
                "   •   À bord : " + summary.BoardedPassengers +
                "   •   Recettes : " + summary.TicketRevenueXof.ToString("N0") + " FCFA";
        if (bookingListText == null) return;
        if (trip.Bookings.Count == 0)
        {
            bookingListText.text = "Aucune réservation pour le moment.";
            return;
        }
        List<string> lines = new List<string>();
        for (int i = 0; i < trip.Bookings.Count; i++)
        {
            PassengerBooking booking = trip.Bookings[i];
            lines.Add(booking.TicketId + "  •  " + booking.PassengerName + "  •  " +
                booking.Origin + " → " + booking.Destination + "  •  " +
                booking.FarePaidXof.ToString("N0") + " FCFA  •  " + booking.Status);
        }
        bookingListText.text = string.Join("\n", lines.ToArray());
    }

    private void ClosePanel()
    {
        if (onClosed != null) onClosed();
        else gameObject.SetActive(false);
    }

    private void SetFeedback(string message, bool error)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.color = error ? new Color(1f, 0.48f, 0.42f) : new Color(0.58f, 0.92f, 0.7f);
    }

    private static void SetOptions(Dropdown dropdown, List<string> options)
    {
        if (dropdown == null) return;
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.interactable = options.Count > 0;
        dropdown.RefreshShownValue();
    }

    private InputField AddInput(Transform parent, string placeholder, float x, float y, float height)
    {
        GameObject root = CreateObject(placeholder, parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 1f);
        rect.anchorMax = new Vector2(0.92f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Image image = root.AddComponent<Image>();
        image.color = new Color(0.10f, 0.15f, 0.22f, 1f);
        InputField field = root.AddComponent<InputField>();
        Text value = CreateText(root.transform, "", 16, TextAnchor.MiddleLeft, Color.white);
        value.rectTransform.offsetMin = new Vector2(12f, 2f);
        value.rectTransform.offsetMax = new Vector2(-12f, -2f);
        Text hint = CreateText(root.transform, placeholder, 15, TextAnchor.MiddleLeft,
            new Color(0.62f, 0.68f, 0.76f));
        hint.rectTransform.offsetMin = new Vector2(12f, 2f);
        hint.rectTransform.offsetMax = new Vector2(-12f, -2f);
        field.textComponent = value;
        field.placeholder = hint;
        field.targetGraphic = image;
        field.lineType = InputField.LineType.SingleLine;
        field.characterLimit = 48;
        return field;
    }

    private Dropdown AddDropdown(Transform parent, string label, float x, float y, float height)
    {
        GameObject root = CreateObject(label, parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 1f);
        rect.anchorMax = new Vector2(0.92f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Image image = root.AddComponent<Image>();
        image.color = new Color(0.10f, 0.15f, 0.22f, 1f);
        Dropdown dropdown = root.AddComponent<Dropdown>();
        Text caption = CreateText(root.transform, label, 15, TextAnchor.MiddleLeft, Color.white);
        caption.rectTransform.offsetMin = new Vector2(12f, 2f);
        caption.rectTransform.offsetMax = new Vector2(-36f, -2f);
        dropdown.captionText = caption;
        dropdown.targetGraphic = image;
        GameObject template = CreateObject("Template", root.transform);
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = Vector2.zero;
        templateRect.sizeDelta = new Vector2(0f, 180f);
        Image templateImage = template.AddComponent<Image>();
        templateImage.color = new Color(0.07f, 0.10f, 0.15f, 1f);
        ScrollRect scroll = template.AddComponent<ScrollRect>();
        GameObject viewport = CreateObject("Viewport", template.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        GameObject content = CreateObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 32f);
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        GameObject item = CreateObject("Item", content.transform);
        item.AddComponent<Toggle>();
        Text itemText = CreateText(item.transform, "Option", 14, TextAnchor.MiddleLeft, Color.white);
        itemText.rectTransform.offsetMin = new Vector2(12f, 0f);
        itemText.rectTransform.offsetMax = new Vector2(-8f, 0f);
        dropdown.template = templateRect;
        dropdown.itemText = itemText;
        template.SetActive(false);
        dropdown.RefreshShownValue();
        return dropdown;
    }

    private void AddButton(Transform parent, string label, float x, float y, float height, UnityEngine.Events.UnityAction action)
    {
        GameObject root = CreateObject(label, parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 1f);
        rect.anchorMax = new Vector2(0.92f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        Image image = root.AddComponent<Image>();
        image.color = new Color(0.12f, 0.35f, 0.56f, 1f);
        Button button = root.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text text = CreateText(root.transform, label, 15, TextAnchor.MiddleCenter, Color.white);
        text.rectTransform.offsetMin = new Vector2(6f, 2f);
        text.rectTransform.offsetMax = new Vector2(-6f, -2f);
    }

    private Text AddText(Transform parent, string value, int size, float x, float y, float height,
        TextAnchor alignment, Color color)
    {
        GameObject root = CreateObject("Text", parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 1f);
        rect.anchorMax = new Vector2(0.92f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(0f, height);
        return CreateText(root.transform, value, size, alignment, color);
    }

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color)
    {
        GameObject root = CreateObject("Label", parent);
        Text text = root.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private GameObject CreateObject(string objectName, Transform parent)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        return root;
    }
}