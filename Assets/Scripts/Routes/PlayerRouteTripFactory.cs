using System;
using System.Collections.Generic;

/// <summary>
/// Validated setup for one player-selected coach journey. This joins the open African route planner,
/// fare generation, passenger manifest and border manager without pretending that a 3D road world
/// or a finished route-selection screen already exists.
/// </summary>
[Serializable]
public sealed class PlayerRouteTripSetup
{
    public RouteItinerary Itinerary { get; private set; }
    public RouteFare Fare { get; private set; }
    public PassengerTrip Passengers { get; private set; }
    public InternationalBorderManager Borders { get; private set; }
    public string CoachId { get; private set; }
    public CoachServiceClass ServiceClass { get; private set; }

    public bool IsInternational { get { return Fare != null && Fare.IsInternational; } }
    public long AnnouncedBorderFeesXof
    {
        get { return Borders == null ? 0L : Borders.GetTotalAnnouncedFeesXof(); }
    }

    internal PlayerRouteTripSetup(RouteItinerary itinerary, RouteFare fare,
        PassengerTrip passengers, InternationalBorderManager borders,
        string coachId, CoachServiceClass serviceClass)
    {
        Itinerary = itinerary;
        Fare = fare;
        Passengers = passengers;
        Borders = borders;
        CoachId = coachId;
        ServiceClass = serviceClass;
    }

    public long GetPassengerFareXof()
    {
        return Fare == null ? 0L : Fare.GetFare(ServiceClass);
    }
}

/// <summary>
/// Creates a complete data-level trip setup from the ordered city IDs chosen by the player.
/// All route distances, fares and border costs remain provisional game values.
/// </summary>
public static class PlayerRouteTripFactory
{
    public static bool TryCreate(string tripId, IList<string> selectedCityIds,
        CompanyProfile company, string coachId, CoachServiceClass serviceClass,
        out PlayerRouteTripSetup setup, out string reason)
    {
        setup = null;
        reason = string.Empty;

        if (company == null)
        {
            reason = "Entreprise introuvable. Créez d'abord votre compagnie.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(tripId))
        {
            reason = "Identifiant de voyage manquant.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(coachId) || !company.OwnsCoach(coachId))
        {
            reason = "Vous devez sélectionner un car que votre compagnie possède.";
            return false;
        }

        CoachDefinition coach = CoachEconomy.FindCoach(coachId);
        if (coach == null || coach.MinimumWheelCount < 6)
        {
            reason = "Seuls les cars de transport de voyageurs d'au moins six roues sont autorisés.";
            return false;
        }

        if (!CoachEconomy.IsServiceAvailable(coach, serviceClass))
        {
            reason = "Cette classe de service n'est pas disponible sur le car choisi.";
            return false;
        }

        if (selectedCityIds == null)
        {
            reason = "Choisissez une ville de départ et une destination.";
            return false;
        }

        RouteItinerary itinerary;
        if (!AfricanRoutePlanner.TryBuildPlayerItinerary(selectedCityIds, out itinerary, out reason))
            return false;

        RouteFare fare = RouteEconomy.CreateFareForItinerary(itinerary);
        if (fare == null)
        {
            reason = "Impossible de calculer le tarif de cet itinéraire.";
            return false;
        }

        try
        {
            PassengerTrip passengers = new PassengerTrip(tripId, coachId, itinerary, serviceClass);
            InternationalBorderManager borders = new InternationalBorderManager(itinerary);
            setup = new PlayerRouteTripSetup(itinerary, fare, passengers, borders, coachId, serviceClass);
            return true;
        }
        catch (ArgumentException)
        {
            reason = "Les paramètres du voyage, du car ou du service sont invalides.";
            return false;
        }
    }
}

