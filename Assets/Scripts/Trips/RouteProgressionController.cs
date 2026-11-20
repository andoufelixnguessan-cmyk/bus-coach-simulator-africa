using System;

/// <summary>
/// Coordinates the ordered stops of a player-created itinerary while the coach is on the road.
/// Passenger exchanges and border clearance are delegated to their existing domain systems.
/// This class contains no Unity dependencies and can be tested independently.
/// </summary>
[Serializable]
public sealed class RouteProgressionController
{
    private readonly PlayerRouteTripSetup setup;
    private readonly CoachTripOperations operations;
    private readonly CompanyProfile company;
    private int nextStopIndex;

    public int NextStopIndex { get { return nextStopIndex; } }
    public int TotalStops { get { return setup.Itinerary.Stops.Count; } }
    public bool AllStopsProcessed { get { return nextStopIndex >= TotalStops; } }
    public RouteStop NextStop
    {
        get
        {
            return AllStopsProcessed ? null : setup.Itinerary.Stops[nextStopIndex];
        }
    }

    public RouteProgressionController(PlayerRouteTripSetup tripSetup,
        CoachTripOperations tripOperations, CompanyProfile tripCompany)
    {
        if (tripSetup == null || tripSetup.Itinerary == null ||
            tripSetup.Itinerary.Stops == null || tripSetup.Itinerary.Stops.Count < 2)
            throw new ArgumentException("Un itinéraire valide est obligatoire.", "tripSetup");
        if (tripOperations == null || tripCompany == null)
            throw new ArgumentException("Les opérations du car et l'entreprise sont obligatoires.");
        if (tripOperations.TripId == null || tripSetup.Passengers == null ||
            tripOperations.TripId != tripSetup.Passengers.TripId ||
            tripOperations.CoachId != tripSetup.CoachId)
            throw new ArgumentException("Le trajet, le manifeste et le car ne correspondent pas.");

        setup = tripSetup;
        operations = tripOperations;
        company = tripCompany;

        // The origin terminal is the departure station. The route progression begins
        // with the first stop after departure; its boarding is handled during preparation.
        nextStopIndex = 1;
    }

    /// <summary>
    /// Processes exactly the next itinerary stop. A border stop cannot be passed until
    /// passenger and vehicle documents are confirmed and the announced fee is paid.
    /// </summary>
    public bool TryProcessNextStop(bool coachStopped, bool parkingBrakeApplied,
        bool passengerDocumentsValid, bool coachDocumentsValid,
        out string summary, out string reason)
    {
        summary = string.Empty;
        reason = string.Empty;

        if (!coachStopped || !parkingBrakeApplied)
        {
            reason = "Immobilisez complètement le car et serrez le frein de stationnement avant de traiter cet arrêt.";
            return false;
        }
        if (operations.Status != CoachTripStatus.OnRoad)
        {
            reason = "Le car doit être en route pour traiter le prochain arrêt.";
            return false;
        }
        if (AllStopsProcessed)
        {
            reason = "Tous les arrêts de cet itinéraire ont déjà été traités.";
            return false;
        }

        RouteStop stop = setup.Itinerary.Stops[nextStopIndex];
        if (stop == null || string.IsNullOrWhiteSpace(stop.Id))
        {
            reason = "Le prochain arrêt de l'itinéraire est invalide.";
            return false;
        }

        if (stop.Type == RouteStopType.BorderCheckpoint)
        {
            string checkpointId = stop.Id.StartsWith("border-stop-", StringComparison.Ordinal)
                ? stop.Id.Substring("border-stop-".Length) : string.Empty;
            if (string.IsNullOrEmpty(checkpointId) || setup.Borders == null)
            {
                reason = "Le poste frontière ne correspond à aucun contrôle prévu.";
                return false;
            }
            if (!setup.Borders.TryClearBorder(checkpointId, passengerDocumentsValid,
                coachDocumentsValid, company, out reason))
            {
                summary = "Contrôle bloqué : " + stop.Name;
                return false;
            }

            nextStopIndex++;
            summary = "Frontière franchie : " + stop.Name +
                ". Documents validés et frais réglés.";
            return true;
        }

        if (stop.PassengersMayBoardOrAlight)
        {
            int alighted;
            int boarded;
            if (!operations.TryProcessPassengerStop(setup.Passengers, stop.Id,
                out alighted, out boarded, out summary, out reason))
                return false;

            nextStopIndex++;
            summary = stop.Name + " — " + summary + ".";
            return true;
        }

        // Rest, fuel and other operational stops are acknowledged in sequence.
        // Their dedicated gameplay interactions can be attached without changing route order.
        nextStopIndex++;
        summary = "Arrêt validé : " + stop.Name + ".";
        return true;
    }

    /// <summary>Closes the trip only after every planned stop and border has been handled.</summary>
    public bool TryCompleteTrip(bool safeDriving, long safeDrivingBonusXof, out string reason)
    {
        reason = string.Empty;
        if (!AllStopsProcessed)
        {
            RouteStop stop = NextStop;
            reason = stop == null
                ? "Le parcours n'est pas encore terminé."
                : "Le prochain arrêt obligatoire est : " + stop.Name + ".";
            return false;
        }
        if (setup.Borders != null && !setup.Borders.AllBordersCleared)
        {
            reason = "Tous les contrôles frontaliers doivent être validés avant la clôture.";
            return false;
        }

        return operations.TryComplete(company, setup.Passengers, safeDriving,
            safeDrivingBonusXof, setup.Borders, out reason);
    }
}
