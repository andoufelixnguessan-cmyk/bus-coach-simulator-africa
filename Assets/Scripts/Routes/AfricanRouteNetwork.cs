using System;
using System.Collections.Generic;

public enum RouteStopType
{
    Terminal = 0,
    PassengerStop = 1,
    RestStop = 2,
    FuelStop = 3,
    BorderCheckpoint = 4
}

[Serializable]
public sealed class RouteStop
{
    public string Id;
    public string Name;
    public string CountryCode;
    public RouteStopType Type;
    public int PlannedStopMinutes;
    public bool PassengersMayBoardOrAlight;

    public RouteStop(string id, string name, string countryCode, RouteStopType type, int minutes, bool passengerExchange)
    {
        Id = id;
        Name = name;
        CountryCode = countryCode;
        Type = type;
        PlannedStopMinutes = Math.Max(0, minutes);
        PassengersMayBoardOrAlight = passengerExchange;
    }
}

[Serializable]
public sealed class BorderCheckpoint
{
    public string Id;
    public string DisplayName;
    public string FromCountryCode;
    public string ToCountryCode;
    public long CoachAndCompanyFeeXof;
    public int EstimatedProcessingMinutes;
    public bool RequiresPassengerDocumentCheck;
    public bool RequiresVehicleDocumentCheck;

    public BorderCheckpoint(string id, string name, string from, string to, long fee, int minutes)
    {
        Id = id;
        DisplayName = name;
        FromCountryCode = from;
        ToCountryCode = to;
        CoachAndCompanyFeeXof = Math.Max(0L, fee);
        EstimatedProcessingMinutes = Math.Max(0, minutes);
        RequiresPassengerDocumentCheck = true;
        RequiresVehicleDocumentCheck = true;
    }
}

[Serializable]
public sealed class RouteItinerary
{
    public string RouteId;
    public string DisplayName;
    public int EstimatedDistanceKm;
    public int EstimatedDrivingMinutes;
    public List<RouteStop> Stops;
    public List<BorderCheckpoint> Borders;

    public RouteItinerary(string id, string name, int distanceKm, int drivingMinutes,
        List<RouteStop> stops, List<BorderCheckpoint> borders)
    {
        RouteId = id;
        DisplayName = name;
        EstimatedDistanceKm = distanceKm;
        EstimatedDrivingMinutes = drivingMinutes;
        Stops = stops ?? new List<RouteStop>();
        Borders = borders ?? new List<BorderCheckpoint>();
    }

    public int TotalPlannedStopMinutes
    {
        get
        {
            int total = 0;
            for (int i = 0; i < Stops.Count; i++) total += Stops[i].PlannedStopMinutes;
            for (int i = 0; i < Borders.Count; i++) total += Borders[i].EstimatedProcessingMinutes;
            return total;
        }
    }

    public int EstimatedTotalMinutes { get { return EstimatedDrivingMinutes + TotalPlannedStopMinutes; } }
}

/// <summary>
/// Original game route network. Distances, durations and fees are balancing estimates, not real-world
/// navigation or current border charges. Replace with surveyed/licensed map data before release.
/// </summary>
public static class AfricanRouteNetwork
{
    // Preset routes are built from the same road-segment planner as player-created itineraries.
    // This guarantees that every preset receives the exact intermediate localities and border order
    // defined for each road segment, including reverse-direction travel.
    private static readonly RouteItinerary[] Routes = BuildRoutes();

    private static RouteItinerary[] BuildRoutes()
    {
        return new RouteItinerary[]
        {
            BuildRoute("abidjan-yamoussoukro", "Abidjan – Yamoussoukro", "abidjan", "yamoussoukro"),
            BuildRoute("abidjan-bouake", "Abidjan – Bouaké", "abidjan", "yamoussoukro", "bouake"),
            BuildRoute("abidjan-korhogo", "Abidjan – Korhogo", "abidjan", "yamoussoukro", "bouake", "korhogo"),
            BuildRoute("abidjan-san-pedro", "Abidjan – San Pedro", "abidjan", "san-pedro"),
            BuildRoute("abidjan-man", "Abidjan – Man", "abidjan", "man"),
            BuildRoute("abidjan-accra", "Abidjan – Accra", "abidjan", "accra"),
            BuildRoute("abidjan-lome", "Abidjan – Lomé", "abidjan", "accra", "lome"),
            BuildRoute("abidjan-cotonou", "Abidjan – Cotonou", "abidjan", "accra", "lome", "cotonou"),
            BuildRoute("abidjan-ouagadougou", "Abidjan – Ouagadougou", "abidjan", "bobo-dioulasso", "ouagadougou"),
            BuildRoute("abidjan-bamako", "Abidjan – Bamako", "abidjan", "sikasso", "bamako"),
            BuildRoute("abidjan-conakry", "Abidjan – Conakry", "abidjan", "man", "nzerekore", "conakry"),
            BuildRoute("accra-lagos", "Accra – Lagos", "accra", "lome", "cotonou", "lagos")
        };
    }

    private static RouteItinerary BuildRoute(string routeId, string displayName, params string[] cityIds)
    {
        RouteItinerary itinerary;
        string reason;
        if (!AfricanRoutePlanner.TryBuildPlayerItinerary(cityIds, out itinerary, out reason))
            throw new InvalidOperationException("Impossible de construire le trajet prédéfini " + routeId + " : " + reason);
        itinerary.RouteId = routeId;
        itinerary.DisplayName = displayName;
        return itinerary;
    }

    public static IReadOnlyList<RouteItinerary> AllRoutes => Routes;

    public static RouteItinerary Find(string routeId)
    {
        if (string.IsNullOrWhiteSpace(routeId)) return null;
        for (int i = 0; i < Routes.Length; i++)
            if (string.Equals(Routes[i].RouteId, routeId, StringComparison.Ordinal)) return Routes[i];
        return null;
    }
}

public enum BorderCrossingStatus
{
    Pending = 0,
    DocumentsChecked = 1,
    Cleared = 2,
    Held = 3
}

[Serializable]
public sealed class BorderCrossingRecord
{
    public string CheckpointId;
    public BorderCrossingStatus Status;
    public bool PassengersDocumentsChecked;
    public bool CoachDocumentsChecked;
    public long FeePaidXof;
    public int ActualProcessingMinutes;
}

/// <summary>Explicit border checks and announced FCFA fees for international trips.</summary>
[Serializable]
public sealed class InternationalBorderManager
{
    private readonly RouteItinerary itinerary;
    private readonly List<BorderCrossingRecord> records = new List<BorderCrossingRecord>();

    public IReadOnlyList<BorderCrossingRecord> Records => records;
    public bool AllBordersCleared
    {
        get
        {
            for (int i = 0; i < records.Count; i++)
                if (records[i].Status != BorderCrossingStatus.Cleared) return false;
            return true;
        }
    }

    public InternationalBorderManager(RouteItinerary route)
    {
        if (route == null) throw new ArgumentNullException("route");
        itinerary = route;
        for (int i = 0; i < route.Borders.Count; i++)
            records.Add(new BorderCrossingRecord {
                CheckpointId = route.Borders[i].Id,
                Status = BorderCrossingStatus.Pending
            });
    }

    public long GetTotalAnnouncedFeesXof()
    {
        long total = 0L;
        for (int i = 0; i < itinerary.Borders.Count; i++)
            total += itinerary.Borders[i].CoachAndCompanyFeeXof;
        return total;
    }

    public long GetTotalFeesPaidXof()
    {
        long total = 0L;
        for (int i = 0; i < records.Count; i++)
            total += Math.Max(0L, records[i].FeePaidXof);
        return total;
    }

    public bool TryClearBorder(string checkpointId, bool passengerDocumentsValid, bool coachDocumentsValid,
        CompanyProfile company, out string reason)
    {
        reason = string.Empty;
        if (string.IsNullOrWhiteSpace(checkpointId) || company == null)
        { reason = "Données de contrôle manquantes."; return false; }

        int index = -1;
        for (int i = 0; i < itinerary.Borders.Count; i++)
            if (itinerary.Borders[i].Id == checkpointId) { index = i; break; }
        if (index < 0) { reason = "Poste frontière inconnu pour cet itinéraire."; return false; }

        BorderCrossingRecord record = records[index];
        if (record.Status == BorderCrossingStatus.Cleared)
        { reason = "Ce contrôle frontalier est déjà validé."; return false; }
        if (!passengerDocumentsValid || !coachDocumentsValid)
        {
            record.Status = BorderCrossingStatus.Held;
            reason = "Passage bloqué : documents des passagers et du car à vérifier.";
            return false;
        }

        long fee = itinerary.Borders[index].CoachAndCompanyFeeXof;
        if (fee > 0L && !company.TrySpend(fee, "Frais annoncés : " + itinerary.Borders[index].DisplayName))
        { reason = "Solde insuffisant pour les frais frontaliers annoncés."; return false; }

        record.PassengersDocumentsChecked = true;
        record.CoachDocumentsChecked = true;
        record.FeePaidXof = fee;
        record.ActualProcessingMinutes = itinerary.Borders[index].EstimatedProcessingMinutes;
        record.Status = BorderCrossingStatus.Cleared;
        return true;
    }
}
