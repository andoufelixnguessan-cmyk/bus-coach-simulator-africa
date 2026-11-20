using System;
using System.Collections.Generic;

/// <summary>Passenger ticket lifecycle for one scheduled, one-way coach trip.</summary>
public enum PassengerTicketStatus
{
    Reserved = 0,
    Boarded = 1,
    Arrived = 2,
    Cancelled = 3,
    NoShow = 4
}

[Serializable]
public sealed class PassengerBooking
{
    public string TicketId;
    public string PassengerName;
    public string Origin;
    public string Destination;
    public string OriginStopId;
    public string DestinationStopId;
    public int OriginStopIndex;
    public int DestinationStopIndex;
    public CoachServiceClass ServiceClass;
    public int SeatNumber;
    public float LuggageWeightKg;
    public long FarePaidXof;
    public PassengerTicketStatus Status;

    public PassengerBooking(string ticketId, string passengerName, string origin, string destination,
        CoachServiceClass serviceClass, int seatNumber, float luggageWeightKg, long farePaidXof,
        string originStopId = null, string destinationStopId = null, int originStopIndex = 0, int destinationStopIndex = 0)
    {
        TicketId = ticketId;
        PassengerName = passengerName;
        Origin = origin;
        Destination = destination;
        OriginStopId = originStopId;
        DestinationStopId = destinationStopId;
        OriginStopIndex = originStopIndex;
        DestinationStopIndex = destinationStopIndex;
        ServiceClass = serviceClass;
        SeatNumber = seatNumber;
        LuggageWeightKg = luggageWeightKg;
        FarePaidXof = farePaidXof;
        Status = PassengerTicketStatus.Reserved;
    }
}

[Serializable]
public sealed class PassengerTripSummary
{
    public int TotalSeats;
    public int ReservedPassengers;
    public int BoardedPassengers;
    public int ArrivedPassengers;
    public int AvailableSeats;
    public float LuggageUsedKg;
    public float LuggageCapacityKg;
    public long TicketRevenueXof;
}

/// <summary>
/// Booking, seat assignment, luggage checks and passenger boarding for one coach trip.
/// Uses the single in-game currency, FCFA (XOF). Call methods from station/management UI.
/// </summary>
[Serializable]
public sealed class PassengerTrip
{
    public const float LuggageAllowanceKgPerPassenger = 25f;
    public const float MaximumSingleBagKg = 32f;

    private readonly string tripId;
    private readonly string coachId;
    private readonly RouteFare route;
    private readonly CoachServiceClass serviceClass;
    private readonly int seatCapacity;
    private readonly float luggageCapacityKg;
    private readonly List<RouteStop> itineraryStops = new List<RouteStop>();
    private readonly List<PassengerBooking> bookings = new List<PassengerBooking>();
    private int nextTicketNumber = 1;
    private int lastProcessedPassengerStopIndex = -1;
    private long ticketRevenueXof;

    public IReadOnlyList<PassengerBooking> Bookings => bookings;
    public string TripId => tripId;
    public string CoachId => coachId;
    public RouteFare Route => route;
    public CoachServiceClass ServiceClass => serviceClass;
    public int SeatCapacity => seatCapacity;

    /// <summary>
    /// Creates a passenger trip directly from a player-selected itinerary so custom international
    /// routes automatically receive an international fare and the minimum-profit guarantee.
    /// </summary>
    public PassengerTrip(string tripId, string coachId, RouteItinerary itinerary, CoachServiceClass serviceClass)
        : this(tripId, coachId, RouteEconomy.CreateFareForItinerary(itinerary), serviceClass)
    {
        if (itinerary != null && itinerary.Stops != null) itineraryStops.AddRange(itinerary.Stops);
    }

    public PassengerTrip(string tripId, string coachId, RouteFare route, CoachServiceClass serviceClass)
    {
        this.tripId = tripId;
        this.coachId = coachId;
        this.route = route;
        this.serviceClass = serviceClass;

        CoachDefinition coach = CoachEconomy.FindCoach(coachId);
        if (string.IsNullOrWhiteSpace(tripId) || coach == null || route == null ||
            !CoachEconomy.IsServiceAvailable(coach, serviceClass))
            throw new ArgumentException("Trajet, car ou classe de service invalide.");

        seatCapacity = (coach.MinSeats + coach.MaxSeats) / 2;
        luggageCapacityKg = seatCapacity * LuggageAllowanceKgPerPassenger;
    }

    public bool TryBookPassenger(string passengerName, CompanyProfile company, float luggageWeightKg,
        out PassengerBooking booking, out string reason)
    {
        string originId = FindFirstPassengerStopId();
        string destinationId = FindLastPassengerStopId();
        return TryBookPassenger(passengerName, company, luggageWeightKg, originId, destinationId,
            out booking, out reason);
    }

    /// <summary>Books a passenger only between boarding/alighting stops on the selected itinerary.</summary>
    public bool TryBookPassenger(string passengerName, CompanyProfile company, float luggageWeightKg,
        string originStopId, string destinationStopId, out PassengerBooking booking, out string reason)
    {
        booking = null;
        reason = string.Empty;
        int originIndex = FindPassengerStopIndex(originStopId);
        int destinationIndex = FindPassengerStopIndex(destinationStopId);
        if (originIndex < 0 || destinationIndex <= originIndex)
        { reason = "Choisissez deux arrêts présents sur ce trajet, dans le bon ordre."; return false; }
        if (company == null) { reason = "Entreprise introuvable."; return false; }
        if (string.IsNullOrWhiteSpace(passengerName)) { reason = "Nom du passager obligatoire."; return false; }
        if (luggageWeightKg < 0f || luggageWeightKg > MaximumSingleBagKg)
        { reason = "Un bagage ne peut pas dépasser 32 kg."; return false; }
        int freeSeat = FindFreeSeat(originIndex, destinationIndex);
        if (freeSeat <= 0)
        { reason = "Aucune place libre sur cette portion du trajet."; return false; }
        if (!CanFitLuggage(luggageWeightKg, originIndex, destinationIndex))
        { reason = "La capacité des soutes est atteinte sur une portion du trajet."; return false; }

        long fare = CalculateStopToStopFare(originIndex, destinationIndex);
        if (fare <= 0L) { reason = "Impossible de calculer le prix de ce billet."; return false; }
        if (!company.AddIncome(fare, "Billet " + itineraryStops[originIndex].Name + " – " +
            itineraryStops[destinationIndex].Name + " - " + passengerName))
        { reason = "Impossible d'enregistrer le paiement du billet."; return false; }

        string ticketId = tripId + "-" + nextTicketNumber.ToString("0000");
        nextTicketNumber++;
        booking = new PassengerBooking(ticketId, passengerName.Trim(),
            itineraryStops[originIndex].Name, itineraryStops[destinationIndex].Name,
            serviceClass, freeSeat, luggageWeightKg, fare,
            itineraryStops[originIndex].Id, itineraryStops[destinationIndex].Id,
            originIndex, destinationIndex);
        bookings.Add(booking);
        ticketRevenueXof += fare;
        return true;
    }

    /// <summary>Processes a selected itinerary stop: alighting first, then boarding.</summary>
    public bool TryProcessPassengerStop(string stopId, out int alighted, out int boarded, out string reason)
    {
        alighted = 0; boarded = 0; reason = string.Empty;
        int stopIndex = FindPassengerStopIndex(stopId);
        if (stopIndex < 0)
        { reason = "Cet arrêt ne permet pas l'échange de passagers sur cet itinéraire."; return false; }
        if (stopIndex <= lastProcessedPassengerStopIndex)
        { reason = "Cet arrêt a déjà été traité ou n'est pas dans le bon ordre."; return false; }
        // Reservations whose boarding stop has already been passed become no-shows.
        for (int i = 0; i < bookings.Count; i++)
        {
            PassengerBooking passenger = bookings[i];
            if (passenger.Status == PassengerTicketStatus.Reserved &&
                passenger.OriginStopIndex < stopIndex)
                passenger.Status = PassengerTicketStatus.NoShow;
        }
        alighted = TryAlightPassengersAtStop(stopId);
        boarded = TryBoardPassengersAtStop(stopId);
        lastProcessedPassengerStopIndex = stopIndex;
        return true;
    }

    /// <summary>Boards all reserved passengers whose origin is the current stop.</summary>
    public int TryBoardPassengersAtStop(string stopId)
    {
        int count = 0;
        for (int i = 0; i < bookings.Count; i++)
        {
            PassengerBooking p = bookings[i];
            if (p.Status == PassengerTicketStatus.Reserved &&
                string.Equals(p.OriginStopId, stopId, StringComparison.Ordinal))
            { p.Status = PassengerTicketStatus.Boarded; count++; }
        }
        return count;
    }

    /// <summary>Lets passengers leave the coach at their own destination stop.</summary>
    public int TryAlightPassengersAtStop(string stopId)
    {
        int count = 0;
        for (int i = 0; i < bookings.Count; i++)
        {
            PassengerBooking p = bookings[i];
            if (p.Status == PassengerTicketStatus.Boarded &&
                string.Equals(p.DestinationStopId, stopId, StringComparison.Ordinal))
            { p.Status = PassengerTicketStatus.Arrived; count++; }
        }
        return count;
    }

    private int FindPassengerStopIndex(string stopId)
    {
        if (string.IsNullOrWhiteSpace(stopId)) return -1;
        for (int i = 0; i < itineraryStops.Count; i++)
            if (itineraryStops[i].PassengersMayBoardOrAlight &&
                string.Equals(itineraryStops[i].Id, stopId, StringComparison.Ordinal)) return i;
        return -1;
    }

    private string FindFirstPassengerStopId()
    {
        for (int i = 0; i < itineraryStops.Count; i++)
            if (itineraryStops[i].PassengersMayBoardOrAlight) return itineraryStops[i].Id;
        return null;
    }

    private string FindLastPassengerStopId()
    {
        for (int i = itineraryStops.Count - 1; i >= 0; i--)
            if (itineraryStops[i].PassengersMayBoardOrAlight) return itineraryStops[i].Id;
        return null;
    }

    private long CalculateStopToStopFare(int originIndex, int destinationIndex)
    {
        int totalPassengerStops = 0;
        int originOrdinal = -1;
        int destinationOrdinal = -1;
        for (int i = 0; i < itineraryStops.Count; i++)
        {
            if (!itineraryStops[i].PassengersMayBoardOrAlight) continue;
            if (i == originIndex) originOrdinal = totalPassengerStops;
            if (i == destinationIndex) destinationOrdinal = totalPassengerStops;
            totalPassengerStops++;
        }
        int totalLegs = totalPassengerStops - 1;
        int passengerLegs = destinationOrdinal - originOrdinal;
        if (totalLegs <= 0 || passengerLegs <= 0) return route.GetFare(serviceClass);
        long fare = (long)Math.Round(route.GetFare(serviceClass) *
            ((double)passengerLegs / totalLegs) / 500.0) * 500L;
        return Math.Max(500L, fare);
    }

    public bool TryBoardPassenger(string ticketId, out string reason)
    {
        reason = string.Empty;
        PassengerBooking booking = FindTicket(ticketId);
        if (booking == null) { reason = "Billet introuvable."; return false; }
        if (booking.Status != PassengerTicketStatus.Reserved)
        { reason = "Ce billet ne peut plus être utilisé pour l'embarquement."; return false; }
        booking.Status = PassengerTicketStatus.Boarded;
        return true;
    }

    public bool TryMarkPassengerArrived(string ticketId, out string reason)
    {
        reason = string.Empty;
        PassengerBooking booking = FindTicket(ticketId);
        if (booking == null) { reason = "Billet introuvable."; return false; }
        if (booking.Status != PassengerTicketStatus.Boarded)
        { reason = "Le passager doit d'abord être embarqué."; return false; }
        booking.Status = PassengerTicketStatus.Arrived;
        return true;
    }

    public bool TryCancelReservation(string ticketId, CompanyProfile company, out string reason)
    {
        reason = string.Empty;
        PassengerBooking booking = FindTicket(ticketId);
        if (booking == null || booking.Status != PassengerTicketStatus.Reserved)
        { reason = "Seule une réservation non embarquée peut être annulée."; return false; }
        if (company == null || !company.TrySpend(booking.FarePaidXof, "Remboursement billet " + booking.TicketId))
        { reason = "Remboursement impossible : solde insuffisant."; return false; }
        booking.Status = PassengerTicketStatus.Cancelled;
        ticketRevenueXof -= booking.FarePaidXof;
        return true;
    }

    public void MarkRemainingReservationsAsNoShow()
    {
        for (int i = 0; i < bookings.Count; i++)
            if (bookings[i].Status == PassengerTicketStatus.Reserved)
                bookings[i].Status = PassengerTicketStatus.NoShow;
    }

    public PassengerTripSummary GetSummary()
    {
        int reserved = 0, boarded = 0, arrived = 0;
        for (int i = 0; i < bookings.Count; i++)
        {
            switch (bookings[i].Status)
            {
                case PassengerTicketStatus.Reserved: reserved++; break;
                case PassengerTicketStatus.Boarded: boarded++; break;
                case PassengerTicketStatus.Arrived: arrived++; break;
            }
        }
        return new PassengerTripSummary
        {
            TotalSeats = seatCapacity,
            ReservedPassengers = reserved,
            BoardedPassengers = boarded,
            ArrivedPassengers = arrived,
            AvailableSeats = Math.Max(0, seatCapacity - GetMaximumPassengersOnAnyLeg()),
            LuggageUsedKg = CurrentLuggageWeightKg(),
            LuggageCapacityKg = luggageCapacityKg,
            TicketRevenueXof = ticketRevenueXof
        };
    }

    private bool CanFitLuggage(float additionalWeightKg, int originIndex, int destinationIndex)
    {
        if (itineraryStops.Count < 2)
            return CurrentLuggageWeightKg() + additionalWeightKg <= luggageCapacityKg;

        float peakWeight = 0f;
        for (int leg = 0; leg < itineraryStops.Count - 1; leg++)
        {
            float weight = 0f;
            for (int i = 0; i < bookings.Count; i++)
            {
                PassengerBooking existing = bookings[i];
                if (existing.Status == PassengerTicketStatus.Cancelled ||
                    existing.Status == PassengerTicketStatus.NoShow) continue;
                if (existing.OriginStopIndex <= leg && existing.DestinationStopIndex > leg)
                    weight += existing.LuggageWeightKg;
            }
            if (originIndex <= leg && destinationIndex > leg) weight += additionalWeightKg;
            if (weight > peakWeight) peakWeight = weight;
        }
        return peakWeight <= luggageCapacityKg;
    }

    private float CurrentLuggageWeightKg()
    {
        if (itineraryStops.Count < 2)
        {
            float total = 0f;
            for (int i = 0; i < bookings.Count; i++)
                if (bookings[i].Status != PassengerTicketStatus.Cancelled &&
                    bookings[i].Status != PassengerTicketStatus.NoShow) total += bookings[i].LuggageWeightKg;
            return total;
        }

        float peakWeight = 0f;
        for (int leg = 0; leg < itineraryStops.Count - 1; leg++)
        {
            float weight = 0f;
            for (int i = 0; i < bookings.Count; i++)
            {
                PassengerBooking booking = bookings[i];
                if (booking.Status == PassengerTicketStatus.Cancelled ||
                    booking.Status == PassengerTicketStatus.NoShow) continue;
                if (booking.OriginStopIndex <= leg && booking.DestinationStopIndex > leg)
                    weight += booking.LuggageWeightKg;
            }
            if (weight > peakWeight) peakWeight = weight;
        }
        return peakWeight;
    }

    /// <summary>Reuses seats for non-overlapping passenger journeys along the same route.</summary>
    private int FindFreeSeat(int originIndex, int destinationIndex)
    {
        for (int seat = 1; seat <= seatCapacity; seat++)
        {
            bool overlaps = false;
            for (int i = 0; i < bookings.Count; i++)
            {
                PassengerBooking existing = bookings[i];
                if (existing.SeatNumber != seat ||
                    existing.Status == PassengerTicketStatus.Cancelled ||
                    existing.Status == PassengerTicketStatus.NoShow) continue;

                bool sameJourneyOverlap = originIndex < existing.DestinationStopIndex &&
                    existing.OriginStopIndex < destinationIndex;
                if (sameJourneyOverlap) { overlaps = true; break; }
            }
            if (!overlaps) return seat;
        }
        return -1;
    }

    private PassengerBooking FindTicket(string ticketId)
    {
        for (int i = 0; i < bookings.Count; i++)
            if (string.Equals(bookings[i].TicketId, ticketId, StringComparison.Ordinal))
                return bookings[i];
        return null;
    }
}
