using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Single orchestration point for preparing, departing, progressing and closing a player trip.
/// Keeps the route, passenger manifest, border checks and company ledger on the same trip session.
/// </summary>
[Serializable]
public sealed class PlayerTripSession
{
    public PlayerRouteTripSetup Setup { get; private set; }
    public CoachTripOperations Operations { get; private set; }
    public RouteProgressionController Progression { get; private set; }
    public CoachTripStatus Status { get { return Operations.Status; } }

    public PlayerTripSession(PlayerRouteTripSetup setup, CompanyProfile company)
    {
        if (setup == null || setup.Itinerary == null || setup.Passengers == null)
            throw new ArgumentException("La préparation complète du voyage est obligatoire.", "setup");
        if (company == null)
            throw new ArgumentNullException("company");

        Setup = setup;
        companyForSession = company;
        Operations = new CoachTripOperations(setup.Passengers.TripId, setup.CoachId);
        Progression = new RouteProgressionController(setup, Operations, company);
    }

    /// <summary>
    /// Validates all trip costs, checks the safety checklist and charges the company once.
    /// If the company balance is insufficient, the session remains ready for a retry.
    /// </summary>
    public bool TryPrepareAndDepart(CoachSafetyChecklist checklist, TripOperatingCosts costs,
        out string reason)
    {
        reason = string.Empty;
        if (costs == null)
        {
            reason = "Le devis des dépenses du voyage est manquant.";
            return false;
        }

        long total;
        if (!TryCalculateCosts(costs, out total))
        {
            reason = "Le devis contient un montant négatif ou dépasse la limite financière autorisée.";
            return false;
        }

        if (Status == CoachTripStatus.Preparing &&
            !Operations.TryMarkReady(checklist, Setup.Passengers, out reason))
            return false;

        if (Status != CoachTripStatus.ReadyToDepart)
        {
            reason = "Ce voyage ne peut plus être préparé ou démarré dans son état actuel.";
            return false;
        }

        TripOperatingCosts validatedCosts = new TripOperatingCosts { FuelXof = costs.FuelXof,
            DriverAndStaffXof = costs.DriverAndStaffXof, TollsAndBorderXof = costs.TollsAndBorderXof,
            FoodAndDrinksXof = costs.FoodAndDrinksXof, MaintenanceReserveXof = costs.MaintenanceReserveXof };
        // CoachTripOperations consumes TotalXof; the checked sum above guarantees it is safe.
        return Operations.TryDepart(companyForSession, validatedCosts, out reason);
    }

    private CompanyProfile companyForSession;

    /// <summary>Sets the company ledger once; exposed separately to keep trip ownership explicit.</summary>
    public void AttachCompany(CompanyProfile company)
    {
        if (company == null) throw new ArgumentNullException("company");
        if (companyForSession != null && !ReferenceEquals(companyForSession, company))
            throw new InvalidOperationException("Une autre entreprise est déjà associée à ce voyage.");
        companyForSession = company;
    }

    public bool TryProcessNextStop(bool coachStopped, bool parkingBrakeApplied,
        bool passengerDocumentsValid, bool coachDocumentsValid,
        out string summary, out string reason)
    {
        return Progression.TryProcessNextStop(coachStopped, parkingBrakeApplied,
            passengerDocumentsValid, coachDocumentsValid, out summary, out reason);
    }

    public bool TryComplete(bool safeDriving, long safeDrivingBonusXof, out string reason)
    {
        return Progression.TryCompleteTrip(safeDriving, safeDrivingBonusXof, out reason);
    }

    public void CaptureSaveState(PlayerTripSaveData data)
    {
        if (data == null) throw new ArgumentNullException("data");
        data.SaveVersion = PlayerTripSaveData.CurrentVersion;
        data.RouteId = Setup.Itinerary.RouteId;
        data.RouteDisplayName = Setup.Itinerary.DisplayName;
        data.EstimatedDistanceKm = Setup.Itinerary.EstimatedDistanceKm;
        data.EstimatedDrivingMinutes = Setup.Itinerary.EstimatedDrivingMinutes;
        data.RouteStops = Setup.Itinerary.Stops.ToArray();
        data.RouteBorders = Setup.Itinerary.Borders.ToArray();
        data.ServiceClass = (int)Setup.ServiceClass;
        data.NextStopIndex = Progression.NextStopIndex;
        Setup.Passengers.CaptureSaveState(data);
        Setup.Borders.CaptureSaveState(data);
        Operations.CaptureSaveState(data);
    }

    public static bool TryRestoreFromSave(PlayerTripSaveData data, CompanyProfile company,
        out PlayerTripSession session, out string reason)
    {
        session = null;
        reason = string.Empty;
        if (data == null || company == null || data.SaveVersion != PlayerTripSaveData.CurrentVersion ||
            string.IsNullOrWhiteSpace(data.TripId) || string.IsNullOrWhiteSpace(data.CoachId) ||
            !company.OwnsCoach(data.CoachId) || data.RouteStops == null || data.RouteStops.Length < 2 ||
            data.RouteBorders == null || !Enum.IsDefined(typeof(CoachServiceClass), data.ServiceClass))
        {
            reason = "La sauvegarde du voyage est incomplète ou ne correspond plus à votre compagnie.";
            return false;
        }

        try
        {
            RouteItinerary itinerary = new RouteItinerary(data.RouteId, data.RouteDisplayName,
                data.EstimatedDistanceKm, data.EstimatedDrivingMinutes,
                new List<RouteStop>(data.RouteStops), new List<BorderCheckpoint>(data.RouteBorders));
            RouteFare fare = RouteEconomy.CreateFareForItinerary(itinerary);
            if (fare == null) { reason = "Impossible de recalculer le tarif de l'itinéraire sauvegardé."; return false; }

            CoachServiceClass serviceClass = (CoachServiceClass)data.ServiceClass;
            PassengerTrip passengers = new PassengerTrip(data.TripId, data.CoachId, itinerary, serviceClass);
            if (!passengers.RestoreSaveState(data, out reason)) return false;

            InternationalBorderManager borders = new InternationalBorderManager(itinerary);
            if (!borders.RestoreSaveState(data, out reason)) return false;

            PlayerRouteTripSetup setup = new PlayerRouteTripSetup(itinerary, fare,
                passengers, borders, data.CoachId, serviceClass);
            PlayerTripSession restored = new PlayerTripSession(setup, company);
            if (!restored.Operations.RestoreSaveState(data, out reason)) return false;
            if (!restored.Progression.RestoreNextStopIndex(data.NextStopIndex, out reason)) return false;

            session = restored;
            return true;
        }
        catch (Exception exception)
        {
            reason = "Impossible de restaurer ce voyage : " + exception.Message;
            return false;
        }
    }

    private static bool TryCalculateCosts(TripOperatingCosts costs, out long total)
    {
        total = 0L;
        if (costs.FuelXof < 0L || costs.DriverAndStaffXof < 0L ||
            costs.TollsAndBorderXof < 0L || costs.FoodAndDrinksXof < 0L ||
            costs.MaintenanceReserveXof < 0L) return false;
        try
        {
            checked
            {
                total = costs.FuelXof + costs.DriverAndStaffXof + costs.TollsAndBorderXof +
                    costs.FoodAndDrinksXof + costs.MaintenanceReserveXof;
            }
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}
