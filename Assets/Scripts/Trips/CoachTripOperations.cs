using System;

[Serializable]
public sealed class CoachSafetyChecklist
{
    public bool TiresAndWheelNutsChecked;
    public bool BrakesChecked;
    public bool LightsAndIndicatorsChecked;
    public bool MirrorsAndWindscreenChecked;
    public bool DoorsAndEmergencyExitsChecked;
    public bool SeatbeltsChecked;
    public bool FireExtinguisherPresent;
    public bool FirstAidKitPresent;
    public bool LuggageSecured;
    public bool DriverRested;

    public bool IsComplete
    {
        get
        {
            return TiresAndWheelNutsChecked && BrakesChecked && LightsAndIndicatorsChecked &&
                MirrorsAndWindscreenChecked && DoorsAndEmergencyExitsChecked && SeatbeltsChecked &&
                FireExtinguisherPresent && FirstAidKitPresent && LuggageSecured && DriverRested;
        }
    }
}

public enum CoachTripStatus
{
    Preparing = 0,
    ReadyToDepart = 1,
    OnRoad = 2,
    Completed = 3,
    Cancelled = 4
}

[Serializable]
public sealed class CoachTripOperations
{
    public string TripId { get; private set; }
    public string CoachId { get; private set; }
    public CoachTripStatus Status { get; private set; }
    public long ChargedOperatingCostsXof { get; private set; }
    public long SafeDrivingBonusXof { get; private set; }

    public CoachTripOperations(string tripId, string coachId)
    {
        if (string.IsNullOrWhiteSpace(tripId) || CoachEconomy.FindCoach(coachId) == null)
            throw new ArgumentException("Identifiant de trajet ou car invalide.");
        TripId = tripId;
        CoachId = coachId;
        Status = CoachTripStatus.Preparing;
    }

    public bool TryMarkReady(CoachSafetyChecklist checklist, PassengerTrip passengers, out string reason)
    {
        reason = string.Empty;
        if (Status != CoachTripStatus.Preparing)
        { reason = "La préparation de ce trajet est déjà terminée."; return false; }
        if (checklist == null || !checklist.IsComplete)
        { reason = "La checklist de sécurité doit être entièrement validée."; return false; }
        if (passengers == null || passengers.TripId != TripId || passengers.CoachId != CoachId)
        { reason = "Le manifeste passagers ne correspond pas à ce trajet ou à ce car."; return false; }
        if (passengers.GetSummary().BoardedPassengers <= 0)
        { reason = "Au moins un passager doit être embarqué."; return false; }

        Status = CoachTripStatus.ReadyToDepart;
        return true;
    }

    public bool TryDepart(CompanyProfile company, TripOperatingCosts costs, out string reason)
    {
        reason = string.Empty;
        if (Status != CoachTripStatus.ReadyToDepart)
        { reason = "Le trajet n'est pas prêt au départ."; return false; }
        if (company == null || costs == null)
        { reason = "Données financières manquantes."; return false; }
        if (costs.TotalXof < 0L)
        { reason = "Les dépenses du trajet sont invalides."; return false; }
        if (!company.TrySpend(costs.TotalXof, "Départ du trajet " + TripId))
        { reason = "Solde insuffisant pour couvrir les dépenses annoncées."; return false; }

        ChargedOperatingCostsXof = costs.TotalXof;
        Status = CoachTripStatus.OnRoad;
        return true;
    }

    public bool TryComplete(CompanyProfile company, PassengerTrip passengers, bool safeDriving,
        long safeDrivingBonusXof, out string reason)
    {
        reason = string.Empty;
        if (Status != CoachTripStatus.OnRoad)
        { reason = "Seul un trajet en cours peut être clôturé."; return false; }
        if (company == null || passengers == null || passengers.TripId != TripId)
        { reason = "Entreprise ou manifeste passagers introuvable."; return false; }
        if (safeDrivingBonusXof < 0L)
        { reason = "La prime indiquée est invalide."; return false; }

        for (int i = 0; i < passengers.Bookings.Count; i++)
        {
            PassengerBooking booking = passengers.Bookings[i];
            if (booking.Status == PassengerTicketStatus.Boarded)
            {
                string arrivalError;
                if (!passengers.TryMarkPassengerArrived(booking.TicketId, out arrivalError))
                { reason = arrivalError; return false; }
            }
        }

        long bonus = safeDriving ? safeDrivingBonusXof : 0L;
        if (bonus > 0L && !company.AddIncome(bonus, "Prime de conduite prudente - " + TripId))
        { reason = "Impossible d'enregistrer la prime."; return false; }

        SafeDrivingBonusXof = bonus;
        Status = CoachTripStatus.Completed;
        return true;
    }

    public bool TryCancelBeforeDeparture(out string reason)
    {
        reason = string.Empty;
        if (Status == CoachTripStatus.OnRoad || Status == CoachTripStatus.Completed)
        { reason = "Un trajet déjà parti ne peut pas être annulé."; return false; }
        if (Status == CoachTripStatus.Cancelled)
        { reason = "Ce trajet est déjà annulé."; return false; }
        Status = CoachTripStatus.Cancelled;
        return true;
    }
}
