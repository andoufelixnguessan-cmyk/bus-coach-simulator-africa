using System;
using System.Collections.Generic;

public enum CoachServiceClass
{
    Economy = 0,
    Comfort = 1,
    Vip = 2
}

[Serializable]
public sealed class CoachDefinition
{
    public string Id;
    public string DisplayName;
    public int MinSeats;
    public int MaxSeats;
    public int MinimumWheelCount;
    public long PurchasePriceXof;
    public int UnlockCompanyLevel;
    public CoachServiceClass HighestServiceClass;

    public CoachDefinition(string id, string displayName, int minSeats, int maxSeats, int minimumWheelCount, long price, int level, CoachServiceClass highestClass)
    {
        Id = id;
        DisplayName = displayName;
        MinSeats = minSeats;
        MaxSeats = maxSeats;
        MinimumWheelCount = Math.Max(6, minimumWheelCount);
        PurchasePriceXof = price;
        UnlockCompanyLevel = level;
        HighestServiceClass = highestClass;
    }
}

/// <summary>Single-currency coach catalogue and company finance foundation. All amounts are FCFA (XOF).</summary>
public static class CoachEconomy
{
    public const long StartingCapitalXof = 24000000L;
    public const string StartingCoachId = "car-moyen";

    private static readonly CoachDefinition[] Definitions =
    {
        new CoachDefinition("car-moyen", "Car interurbain 6 roues", 31, 39, 6, 22000000L, 1, CoachServiceClass.Comfort),
        new CoachDefinition("autocar-standard", "Autocar standard 6 roues", 40, 49, 6, 32000000L, 2, CoachServiceClass.Comfort),
        new CoachDefinition("autocar-confort", "Autocar confort 6 roues", 45, 53, 6, 42000000L, 3, CoachServiceClass.Vip),
        new CoachDefinition("grand-tourisme", "Autocar grand tourisme 6 roues", 49, 57, 6, 58000000L, 4, CoachServiceClass.Vip),
        new CoachDefinition("autocar-premium", "Autocar premium longue distance 6 roues", 51, 59, 6, 75000000L, 6, CoachServiceClass.Vip),
        new CoachDefinition("double-etage-tourisme", "Autocar double étage tourisme 10 roues", 70, 83, 10, 95000000L, 8, CoachServiceClass.Vip)
    };

    public static IReadOnlyList<CoachDefinition> AllCoaches => Definitions;

    public static CoachDefinition FindCoach(string coachId)
    {
        if (string.IsNullOrEmpty(coachId)) return null;
        for (int i = 0; i < Definitions.Length; i++)
            if (string.Equals(Definitions[i].Id, coachId, StringComparison.Ordinal)) return Definitions[i];
        return null;
    }

    public static CompanyProfile CreateNewCompany()
    {
        var company = new CompanyProfile(StartingCapitalXof);
        CoachDefinition starter = FindCoach(StartingCoachId);
        if (starter != null && company.TryPurchaseCoach(starter.Id, 1))
            company.RecordStartingCoach(starter.Id);
        return company;
    }

    public static bool IsServiceAvailable(CoachDefinition coach, CoachServiceClass serviceClass)
    {
        return coach != null && serviceClass <= coach.HighestServiceClass;
    }
}

[Serializable]
public sealed class FinanceEntry
{
    public string Label;
    public long AmountXof;
    public long BalanceAfterXof;
    public bool IsIncome;

    public FinanceEntry(string label, long amount, long balance, bool income)
    { Label = label; AmountXof = amount; BalanceAfterXof = balance; IsIncome = income; }
}

/// <summary>Runtime company data. Persist this object with the game's save system.</summary>
[Serializable]
public sealed class CompanyProfile
{
    public long BalanceXof { get; private set; }
    public int CompanyLevel { get; private set; }
    public List<string> OwnedCoachIds { get; private set; }
    public List<FinanceEntry> FinanceHistory { get; private set; }
    public string StartingCoachId { get; private set; }

    public CompanyProfile(long startingBalanceXof)
    {
        BalanceXof = Math.Max(0L, startingBalanceXof);
        CompanyLevel = 1;
        OwnedCoachIds = new List<string>();
        FinanceHistory = new List<FinanceEntry>();
    }

    public bool OwnsCoach(string coachId) { return OwnedCoachIds.Contains(coachId); }

    public bool TryPurchaseCoach(string coachId, int companyLevel)
    {
        CoachDefinition coach = CoachEconomy.FindCoach(coachId);
        if (coach == null || OwnsCoach(coachId) || companyLevel < coach.UnlockCompanyLevel || BalanceXof < coach.PurchasePriceXof)
            return false;
        BalanceXof -= coach.PurchasePriceXof;
        OwnedCoachIds.Add(coachId);
        FinanceHistory.Add(new FinanceEntry("Achat : " + coach.DisplayName, coach.PurchasePriceXof, BalanceXof, false));
        return true;
    }

    public bool TrySpend(long amountXof, string reason)
    {
        if (amountXof <= 0L || BalanceXof < amountXof) return false;
        BalanceXof -= amountXof;
        FinanceHistory.Add(new FinanceEntry(reason ?? "Dépense", amountXof, BalanceXof, false));
        return true;
    }

    public bool AddIncome(long amountXof, string reason)
    {
        if (amountXof <= 0L || BalanceXof > long.MaxValue - amountXof) return false;
        BalanceXof += amountXof;
        FinanceHistory.Add(new FinanceEntry(reason ?? "Recette", amountXof, BalanceXof, true));
        return true;
    }

    public void SetCompanyLevel(int level) { CompanyLevel = Math.Max(1, level); }
    internal void RecordStartingCoach(string coachId) { StartingCoachId = coachId; }
}

[Serializable]
public sealed class RouteFare
{
    public string RouteId;
    public string DisplayName;
    public long EconomyXof;
    public long ComfortXof;
    public long VipXof;
    public bool IsInternational;

    public RouteFare(string id, string name, long economy, long comfort, long vip, bool isInternational = false)
    { RouteId = id; DisplayName = name; EconomyXof = economy; ComfortXof = comfort; VipXof = vip; IsInternational = isInternational; }

    public long GetFare(CoachServiceClass serviceClass)
    {
        switch (serviceClass)
        {
            case CoachServiceClass.Comfort: return ComfortXof;
            case CoachServiceClass.Vip: return VipXof;
            default: return EconomyXof;
        }
    }
}

[Serializable]
public sealed class TripOperatingCosts
{
    public long FuelXof;
    public long DriverAndStaffXof;
    public long TollsAndBorderXof;
    public long FoodAndDrinksXof;
    public long MaintenanceReserveXof;

    public long TotalXof
    {
        get { return FuelXof + DriverAndStaffXof + TollsAndBorderXof + FoodAndDrinksXof + MaintenanceReserveXof; }
    }
}

[Serializable]
public sealed class TripFinancialQuote
{
    public int Seats;
    public int ExpectedPassengers;
    public long TicketRevenueXof;
    public long OperatingCostsXof;
    public long EstimatedProfitXof;
    public long InternationalGuaranteeTopUpXof;
    public bool IsProfitable { get { return EstimatedProfitXof >= 0L; } }
}

/// <summary>Calculates a transparent one-way estimate; actual ticket sales determine final revenue.</summary>
public static class RouteEconomy
{
    public const long MinimumInternationalProfitXof = 500000L;

    public static long CalculateInternationalGuaranteeTopUp(RouteFare route, long ticketRevenueXof, long operatingCostsXof)
    {
        if (route == null || !route.IsInternational || ticketRevenueXof <= 0L) return 0L;
        long currentProfit = ticketRevenueXof - Math.Max(0L, operatingCostsXof);
        return Math.Max(0L, MinimumInternationalProfitXof - currentProfit);
    }

    private static readonly RouteFare[] Fares =
    {
        new RouteFare("abidjan-yamoussoukro", "Abidjan – Yamoussoukro", 5000, 7000, 10000),
        new RouteFare("abidjan-bouake", "Abidjan – Bouaké", 8000, 12000, 16000),
        new RouteFare("abidjan-korhogo", "Abidjan – Korhogo", 12000, 16000, 22000),
        new RouteFare("abidjan-san-pedro", "Abidjan – San Pedro", 10000, 14000, 18000),
        new RouteFare("abidjan-man", "Abidjan – Man", 12000, 17000, 23000),
        new RouteFare("abidjan-accra", "Abidjan – Accra", 15000, 20000, 28000, true),
        new RouteFare("abidjan-lome", "Abidjan – Lomé", 22000, 30000, 40000, true),
        new RouteFare("abidjan-cotonou", "Abidjan – Cotonou", 28000, 38000, 50000, true),
        new RouteFare("abidjan-ouagadougou", "Abidjan – Ouagadougou", 30000, 42000, 55000, true),
        new RouteFare("abidjan-bamako", "Abidjan – Bamako", 35000, 48000, 65000, true),
        new RouteFare("abidjan-conakry", "Abidjan – Conakry", 45000, 60000, 80000, true),
        new RouteFare("accra-lagos", "Accra – Lagos", 30000, 42000, 55000, true)
    };

    public static IReadOnlyList<RouteFare> AllRoutes => Fares;

    /// <summary>
    /// Creates a fare for a player-built itinerary. Known catalogue routes keep their curated fares;
    /// all other routes receive a distance-based provisional fare. International status is derived
    /// from border crossings and country changes, never from a user-supplied flag.
    /// </summary>
    public static RouteFare CreateFareForItinerary(RouteItinerary itinerary)
    {
        if (itinerary == null || string.IsNullOrWhiteSpace(itinerary.RouteId))
            return null;

        RouteFare catalogueFare = FindRoute(itinerary.RouteId);
        if (catalogueFare != null) return catalogueFare;

        bool international = IsInternationalItinerary(itinerary);
        long distanceKm = Math.Max(1L, itinerary.EstimatedDistanceKm);
        long minimumFare = international ? 15000L : 5000L;

        // Provisional game-balancing rates: 30 / 40 / 55 XOF per km.
        // Round to the nearest 500 XOF to keep station prices readable.
        long economy = Math.Max(minimumFare, RoundFare(distanceKm * 30L));
        long comfort = Math.Max(economy, Math.Max(minimumFare, RoundFare(distanceKm * 40L)));
        long vip = Math.Max(comfort, Math.Max(minimumFare, RoundFare(distanceKm * 55L)));

        return new RouteFare(itinerary.RouteId, itinerary.DisplayName,
            economy, comfort, vip, international);
    }

    public static bool IsInternationalItinerary(RouteItinerary itinerary)
    {
        if (itinerary == null) return false;
        if (itinerary.Borders != null && itinerary.Borders.Count > 0) return true;

        if (itinerary.Stops == null || itinerary.Stops.Count < 2) return false;
        string firstCountry = null;
        for (int i = 0; i < itinerary.Stops.Count; i++)
        {
            string country = itinerary.Stops[i].CountryCode;
            if (string.IsNullOrWhiteSpace(country)) continue;
            if (firstCountry == null) firstCountry = country;
            else if (!string.Equals(firstCountry, country, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static long RoundFare(long amount)
    {
        return ((amount + 250L) / 500L) * 500L;
    }

    public static RouteFare FindRoute(string routeId)
    {
        if (string.IsNullOrEmpty(routeId)) return null;
        for (int i = 0; i < Fares.Length; i++)
            if (string.Equals(Fares[i].RouteId, routeId, StringComparison.Ordinal)) return Fares[i];
        return null;
    }

    public static TripFinancialQuote CreateQuote(CoachDefinition coach, RouteFare route, CoachServiceClass serviceClass, int occupancyPercent, TripOperatingCosts costs)
    {
        if (coach == null || route == null || costs == null || !CoachEconomy.IsServiceAvailable(coach, serviceClass)) return null;
        int percent = Math.Max(0, Math.Min(100, occupancyPercent));
        int seats = (coach.MinSeats + coach.MaxSeats) / 2;
        int passengers = (seats * percent + 50) / 100;
        long revenue = passengers * route.GetFare(serviceClass);
        long totalCosts = costs.TotalXof;
        long topUp = CalculateInternationalGuaranteeTopUp(route, revenue, totalCosts);
        return new TripFinancialQuote
        {
            Seats = seats,
            ExpectedPassengers = passengers,
            TicketRevenueXof = revenue,
            OperatingCostsXof = totalCosts,
            InternationalGuaranteeTopUpXof = topUp,
            EstimatedProfitXof = revenue + topUp - totalCosts
        };
    }
}