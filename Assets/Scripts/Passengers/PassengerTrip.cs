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
    public CoachServiceClass ServiceClass;
    public int SeatNumber;
    public float LuggageWeightKg;
    public long FarePaidXof;
    public PassengerTicketStatus Status;

    public PassengerBooking(string ticketId, string passengerName, string origin, string destination,
        CoachServiceClass serviceClass, int seatNumber, float luggageWeightKg, long farePaidXof)
    {
        TicketId = ticketId;
        PassengerName = passengerName;
        Origin = origin;
        Destination = destination;
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
    private readonly List<PassengerBooking> bookings = new List<PassengerBooking>();
    private int nextTicketNumber = 1;
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
        booking = null;
        reason = string.Empty;

        if (company == null) { reason = "Entreprise introuvable."; return false; }
        if (string.IsNullOrWhiteSpace(passengerName)) { reason = "Nom du passager obligatoire."; return false; }
        if (luggageWeightKg < 0f || luggageWeightKg > MaximumSingleBagKg)
        { reason = "Un bagage ne peut pas dépasser 32 kg."; return false; }
        if (ReservedPassengerCount() >= seatCapacity)
        { reason = "Le car est complet."; return false; }
        if (CurrentLuggageWeightKg() + luggageWeightKg > luggageCapacityKg)
        { reason = "La capacité totale des soutes est atteinte."; return false; }

        int seat = FindFreeSeat();
        long fare = route.GetFare(serviceClass);
        if (!company.AddIncome(fare, "Billet " + route.DisplayName + " - " + passengerName))
        { reason = "Impossible d'enregistrer le paiement du billet."; return false; }

        string ticketId = tripId + "-" + nextTicketNumber.ToString("0000");
        nextTicketNumber++;
        booking = new PassengerBooking(ticketId, passengerName.Trim(), route.DisplayName.Split('–')[0].Trim(),
            route.DisplayName.Split('–').Length > 1 ? route.DisplayName.Split('–')[1].Trim() : route.DisplayName,
            serviceClass, seat, luggageWeightKg, fare);
        bookings.Add(booking);
        ticketRevenueXof += fare;
        return true;
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
            AvailableSeats = Math.Max(0, seatCapacity - reserved - boarded - arrived),
            LuggageUsedKg = CurrentLuggageWeightKg(),
            LuggageCapacityKg = luggageCapacityKg,
            TicketRevenueXof = ticketRevenueXof
        };
    }

    private int ReservedPassengerCount()
    {
        int count = 0;
        for (int i = 0; i < bookings.Count; i++)
            if (bookings[i].Status != PassengerTicketStatus.Cancelled &&
                bookings[i].Status != PassengerTicketStatus.NoShow) count++;
        return count;
    }

    private float CurrentLuggageWeightKg()
    {
        float total = 0f;
        for (int i = 0; i < bookings.Count; i++)
            if (bookings[i].Status != PassengerTicketStatus.Cancelled) total += bookings[i].LuggageWeightKg;
        return total;
    }

    private int FindFreeSeat()
    {
        for (int seat = 1; seat <= seatCapacity; seat++)
        {
            bool used = false;
            for (int i = 0; i < bookings.Count; i++)
                if (bookings[i].SeatNumber == seat &&
                    bookings[i].Status != PassengerTicketStatus.Cancelled &&
                    bookings[i].Status != PassengerTicketStatus.NoShow)
                { used = true; break; }
            if (!used) return seat;
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
