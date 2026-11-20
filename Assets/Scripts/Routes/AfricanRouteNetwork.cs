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
    private static readonly RouteItinerary[] Routes =
    {
        new RouteItinerary("abidjan-yamoussoukro", "Abidjan – Yamoussoukro", 240, 210,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("tiassale-rest", "Aire de repos de Tiassalé", "CI", RouteStopType.RestStop, 15, false),
                new RouteStop("yak-terminal", "Gare de Yamoussoukro", "CI", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint>()),

        new RouteItinerary("abidjan-bouake", "Abidjan – Bouaké", 350, 300,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("yak-stop", "Yamoussoukro", "CI", RouteStopType.PassengerStop, 15, true),
                new RouteStop("bouake-terminal", "Gare de Bouaké", "CI", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint>()),

        new RouteItinerary("abidjan-korhogo", "Abidjan – Korhogo", 635, 540,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("yak-stop", "Yamoussoukro", "CI", RouteStopType.PassengerStop, 15, true),
                new RouteStop("bouake-stop", "Bouaké", "CI", RouteStopType.PassengerStop, 20, true),
                new RouteStop("korhogo-terminal", "Gare de Korhogo", "CI", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint>()),

        new RouteItinerary("abidjan-san-pedro", "Abidjan – San Pedro", 350, 330,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("sassandra-rest", "Aire de repos de Sassandra", "CI", RouteStopType.RestStop, 15, false),
                new RouteStop("sanpedro-terminal", "Gare de San Pedro", "CI", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint>()),

        new RouteItinerary("abidjan-man", "Abidjan – Man", 570, 510,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("daloa-stop", "Daloa", "CI", RouteStopType.PassengerStop, 20, true),
                new RouteStop("man-terminal", "Gare de Man", "CI", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint>()),

        new RouteItinerary("abidjan-accra", "Abidjan – Accra", 500, 420,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("noe-border", "Poste frontière Noé – Elubo", "CI", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("takoradi-stop", "Takoradi", "GH", RouteStopType.PassengerStop, 20, true),
                new RouteStop("accra-terminal", "Gare d'Accra", "GH", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("noe-elubo", "Frontière Noé – Elubo", "CI", "GH", 15000L, 60)
            }),

        new RouteItinerary("abidjan-lome", "Abidjan – Lomé", 900, 780,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("noe-border", "Poste frontière Noé – Elubo", "CI", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("accra-stop", "Accra", "GH", RouteStopType.PassengerStop, 25, true),
                new RouteStop("aflao-border", "Poste frontière Aflao – Kodjoviakopé", "GH", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("lome-terminal", "Gare de Lomé", "TG", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("noe-elubo", "Frontière Noé – Elubo", "CI", "GH", 15000L, 60),
                new BorderCheckpoint("aflao-kodjoviakope", "Frontière Aflao – Kodjoviakopé", "GH", "TG", 15000L, 60)
            }),

        new RouteItinerary("abidjan-cotonou", "Abidjan – Cotonou", 1050, 900,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("noe-border", "Poste frontière Noé – Elubo", "CI", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("accra-stop", "Accra", "GH", RouteStopType.PassengerStop, 25, true),
                new RouteStop("aflao-border", "Poste frontière Aflao – Kodjoviakopé", "GH", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("lome-stop", "Lomé", "TG", RouteStopType.PassengerStop, 25, true),
                new RouteStop("hillacondji-border", "Frontière Hillacondji – Sanvee Condji", "TG", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("cotonou-terminal", "Gare de Cotonou", "BJ", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("noe-elubo", "Frontière Noé – Elubo", "CI", "GH", 15000L, 60),
                new BorderCheckpoint("aflao-kodjoviakope", "Frontière Aflao – Kodjoviakopé", "GH", "TG", 15000L, 60),
                new BorderCheckpoint("hillacondji-sanvee", "Frontière Hillacondji – Sanvee Condji", "TG", "BJ", 15000L, 60)
            }),

        new RouteItinerary("abidjan-ouagadougou", "Abidjan – Ouagadougou", 1120, 1020,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("korhogo-stop", "Korhogo", "CI", RouteStopType.PassengerStop, 25, true),
                new RouteStop("pogo-border", "Frontière Pogo – Zégoua", "CI", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("bobo-stop", "Bobo-Dioulasso", "BF", RouteStopType.PassengerStop, 25, true),
                new RouteStop("ouaga-terminal", "Gare de Ouagadougou", "BF", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("pogo-zegoua", "Frontière Pogo – Zégoua", "CI", "BF", 18000L, 75)
            }),

        new RouteItinerary("abidjan-bamako", "Abidjan – Bamako", 1200, 1080,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("korhogo-stop", "Korhogo", "CI", RouteStopType.PassengerStop, 25, true),
                new RouteStop("pogo-border", "Frontière Pogo – Zégoua", "CI", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("sikasso-stop", "Sikasso", "ML", RouteStopType.PassengerStop, 25, true),
                new RouteStop("bamako-terminal", "Gare de Bamako", "ML", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("pogo-zegoua", "Frontière Pogo – Zégoua", "CI", "ML", 18000L, 75)
            }),

        new RouteItinerary("abidjan-conakry", "Abidjan – Conakry", 1550, 1380,
            new List<RouteStop> {
                new RouteStop("abj-terminal", "Gare d'Abidjan", "CI", RouteStopType.Terminal, 20, true),
                new RouteStop("man-stop", "Man", "CI", RouteStopType.PassengerStop, 25, true),
                new RouteStop("danane-stop", "Danané", "CI", RouteStopType.PassengerStop, 15, true),
                new RouteStop("nzerekore-stop", "N'Zérékoré", "GN", RouteStopType.PassengerStop, 25, true),
                new RouteStop("conakry-terminal", "Gare de Conakry", "GN", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("danane-liberia-guinea", "Contrôle frontalier régional", "CI", "GN", 20000L, 90)
            }),

        new RouteItinerary("accra-lagos", "Accra – Lagos", 460, 420,
            new List<RouteStop> {
                new RouteStop("accra-terminal", "Gare d'Accra", "GH", RouteStopType.Terminal, 20, true),
                new RouteStop("aflao-border", "Poste frontière Aflao – Kodjoviakopé", "GH", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("lome-stop", "Lomé", "TG", RouteStopType.PassengerStop, 20, true),
                new RouteStop("hillacondji-border", "Frontière Hillacondji – Sanvee Condji", "TG", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("cotonou-stop", "Cotonou", "BJ", RouteStopType.PassengerStop, 20, true),
                new RouteStop("seme-border", "Frontière Sèmè – Krake", "BJ", RouteStopType.BorderCheckpoint, 0, false),
                new RouteStop("lagos-terminal", "Gare de Lagos", "NG", RouteStopType.Terminal, 20, true)
            }, new List<BorderCheckpoint> {
                new BorderCheckpoint("aflao-kodjoviakope", "Frontière Aflao – Kodjoviakopé", "GH", "TG", 15000L, 60),
                new BorderCheckpoint("hillacondji-sanvee", "Frontière Hillacondji – Sanvee Condji", "TG", "BJ", 15000L, 60),
                new BorderCheckpoint("seme-krake", "Frontière Sèmè – Krake", "BJ", "NG", 18000L, 75)
            })
    };

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
