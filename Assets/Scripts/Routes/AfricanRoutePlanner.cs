using System;
using System.Collections.Generic;

/// <summary>
/// Open route planner: the player chooses the origin, destination and every intermediate city.
/// Each consecutive pair must be connected by a road segment in this original game network.
/// Distances, times and border fees are provisional balancing estimates, not navigation data.
/// </summary>
public static class AfricanRoutePlanner
{
    [Serializable]
    public sealed class City
    {
        public string Id;
        public string Name;
        public string CountryCode;
        public City(string id, string name, string country)
        { Id = id; Name = name; CountryCode = country; }
    }

    [Serializable]
    public sealed class RoadSegment
    {
        public string FromCityId;
        public string ToCityId;
        public int DistanceKm;
        public int DrivingMinutes;
        public string BorderId;
        public string BorderName;
        public long BorderFeeXof;
        public int BorderMinutes;
        public string[] IntermediateLocalities;
        public string[] IntermediateLocalityCountryCodes;
        public int BorderAfterLocalityCount;

        public RoadSegment(string from, string to, int km, int minutes,
            string borderId = null, string borderName = null, long fee = 0L, int borderMinutes = 0, params string[] intermediateLocalities)
        {
            FromCityId = from; ToCityId = to;
            DistanceKm = km; DrivingMinutes = minutes;
            BorderId = borderId; BorderName = borderName;
            BorderFeeXof = fee; BorderMinutes = borderMinutes;
            IntermediateLocalities = intermediateLocalities ?? new string[0];
            IntermediateLocalityCountryCodes = new string[IntermediateLocalities.Length];
            for (int i = 0; i < IntermediateLocalityCountryCodes.Length; i++)
                IntermediateLocalityCountryCodes[i] = null;
            BorderAfterLocalityCount = IntermediateLocalities.Length;
        }

        /// <summary>Sets the country for each locality and the exact position of the border stop.</summary>
        public RoadSegment ConfigureLocalities(int borderAfterLocalityCount, params string[] countryCodes)
        {
            BorderAfterLocalityCount = Math.Max(0, Math.Min(IntermediateLocalities.Length, borderAfterLocalityCount));
            if (countryCodes != null && countryCodes.Length == IntermediateLocalities.Length)
                IntermediateLocalityCountryCodes = (string[])countryCodes.Clone();
            return this;
        }
    }

    private static readonly City[] Cities =
    {
        new City("abidjan", "Abidjan", "CI"),
        new City("yamoussoukro", "Yamoussoukro", "CI"),
        new City("bouake", "Bouaké", "CI"),
        new City("korhogo", "Korhogo", "CI"),
        new City("san-pedro", "San Pedro", "CI"),
        new City("man", "Man", "CI"),
        new City("accra", "Accra", "GH"),
        new City("kumasi", "Kumasi", "GH"),
        new City("tamale", "Tamale", "GH"),
        new City("lome", "Lomé", "TG"),
        new City("cotonou", "Cotonou", "BJ"),
        new City("porto-novo", "Porto-Novo", "BJ"),
        new City("lagos", "Lagos", "NG"),
        new City("ibadan", "Ibadan", "NG"),
        new City("ouagadougou", "Ouagadougou", "BF"),
        new City("bobo-dioulasso", "Bobo-Dioulasso", "BF"),
        new City("bamako", "Bamako", "ML"),
        new City("sikasso", "Sikasso", "ML"),
        new City("conakry", "Conakry", "GN"),
        new City("nzerekore", "N'Zérékoré", "GN"),
        new City("monrovia", "Monrovia", "LR"),
        new City("freetown", "Freetown", "SL"),
        new City("dakar", "Dakar", "SN"),
        new City("banjul", "Banjul", "GM"),
        new City("niamey", "Niamey", "NE"),
        new City("kano", "Kano", "NG"),
        new City("douala", "Douala", "CM"),
        new City("yaounde", "Yaoundé", "CM"),
        new City("libreville", "Libreville", "GA")
    };

    // Bidirectional road links. Game estimates only; road geometry and legal corridors need later validation.
    private static readonly RoadSegment[] Segments =
    {
        new RoadSegment("abidjan","yamoussoukro",240,210, null, null, 0L, 0, "Anyama", "Singrobo", "Tiassalé", "N’Douci", "Toumodi"),
        new RoadSegment("yamoussoukro","bouake",110,100, null, null, 0L, 0, "Tiébissou", "Djébonoua"),
        new RoadSegment("bouake","korhogo",285,250, null, null, 0L, 0, "Katiola", "Niakaramandougou", "Tafiré", "Ferkessédougou"),
        new RoadSegment("abidjan","san-pedro",350,330, null, null, 0L, 0, "Dabou", "Grand-Lahou", "Sassandra"),
        new RoadSegment("abidjan","man",570,510, null, null, 0L, 0, "Dabou", "Divo", "Gagnoa", "Daloa", "Duékoué"),
        new RoadSegment("man","nzerekore",260,240,"ci-gn-man","Corridor Man – N'Zérékoré",20000,90, "Danané", "Lola").ConfigureLocalities(1, "CI", "GN"),
        new RoadSegment("nzerekore","conakry",700,660, null, null, 0L, 0, "Macenta", "Kissidougou", "Mamou"),
        new RoadSegment("abidjan","accra",500,420,"ci-gh-noe","Frontière Noé – Elubo",15000,60, "Grand-Bassam", "Aboisso", "Noé", "Elubo", "Takoradi", "Cape Coast", "Winneba", "Tema").ConfigureLocalities(3, "CI", "CI", "CI", "GH", "GH", "GH", "GH", "GH"),
        new RoadSegment("accra","kumasi",250,210, null, null, 0L, 0, "Nsawam", "Nkawkaw"),
        new RoadSegment("kumasi","tamale",380,330, null, null, 0L, 0, "Techiman", "Kintampo"),
        new RoadSegment("accra","lome",200,180,"gh-tg-aflao","Frontière Aflao – Kodjoviakopé",15000,60, "Tema", "Kasseh", "Akatsi", "Akanu", "Noépé", "Tsévié").ConfigureLocalities(4, "GH", "GH", "GH", "GH", "TG", "TG"),
        new RoadSegment("lome","cotonou",155,135,"tg-bj-hillacondji","Frontière Hillacondji – Sanvee Condji",15000,60, "Tsévié", "Noépé", "Athiémé", "Ouidah", "Godomey").ConfigureLocalities(2, "TG", "TG", "BJ", "BJ", "BJ"),
        new RoadSegment("cotonou","porto-novo",35,35, null, null, 0L, 0, "Sèmè-Kpodji"),
        new RoadSegment("cotonou","lagos",115,120,"bj-ng-seme","Frontière Sèmè – Krake",18000,75, "Porto-Novo", "Sèmè", "Badagry", "Agbara").ConfigureLocalities(2, "BJ", "BJ", "NG", "NG"),
        new RoadSegment("lagos","ibadan",130,120, null, null, 0L, 0, "Mowe", "Ibafo"),
        new RoadSegment("lagos","kano",990,900, null, null, 0L, 0, "Ibadan", "Ilorin", "Jebba", "Kaduna"),
        new RoadSegment("abidjan","bobo-dioulasso",850,780,"ci-bf-pogo","Frontière Pogo – Zégoua",18000,75, "Yamoussoukro", "Bouaké", "Katiola", "Ferkessédougou", "Niangoloko", "Banfora").ConfigureLocalities(4, "CI", "CI", "CI", "CI", "BF", "BF"),
        new RoadSegment("bobo-dioulasso","ouagadougou",360,330, null, null, 0L, 0, "Koudougou"),
        new RoadSegment("abidjan","sikasso",900,840,"ci-ml-pogo","Corridor Côte d'Ivoire – Mali",18000,75, "Yamoussoukro", "Bouaké", "Korhogo", "Ferkessédougou"),
        new RoadSegment("sikasso","bamako",375,330, null, null, 0L, 0, "Bougouni", "Koumantou"),
        new RoadSegment("abidjan","monrovia",900,840,"ci-lr-border","Frontière Côte d'Ivoire – Liberia",20000,90, "Grand-Lahou", "Sassandra", "San Pedro", "Tabou", "Harper", "Greenville", "Buchanan").ConfigureLocalities(4, "CI", "CI", "CI", "CI", "LR", "LR", "LR"),
        new RoadSegment("monrovia","freetown",480,450,"lr-sl-border","Frontière Liberia – Sierra Leone",18000,80, "Gbarnga", "Ganta", "Bo Waterside", "Kenema").ConfigureLocalities(3, "LR", "LR", "LR", "SL"),
        new RoadSegment("freetown","conakry",160,150,"sl-gn-border","Frontière Sierra Leone – Guinée",18000,80, "Masiaka", "Kambia", "Forécariah").ConfigureLocalities(2, "SL", "SL", "GN"),
        new RoadSegment("dakar","banjul",165,150,"sn-gm-border","Frontière Sénégal – Gambie",12000,50, "Kaolack", "Soma"),
        new RoadSegment("dakar","bamako",1250,1140,"sn-ml-border","Frontière Sénégal – Mali",22000,90, "Tambacounda", "Kidira", "Kayes", "Kita").ConfigureLocalities(2, "SN", "SN", "ML", "ML"),
        new RoadSegment("ouagadougou","niamey",520,480,"bf-ne-border","Frontière Burkina Faso – Niger",18000,75, "Koupéla", "Fada N’Gourma", "Torodi").ConfigureLocalities(2, "BF", "BF", "NE"),
        new RoadSegment("ouagadougou","tamale",450,420,"bf-gh-border","Frontière Burkina Faso – Ghana",18000,75, "Paga", "Bawku").ConfigureLocalities(1, "BF", "GH"),
        new RoadSegment("niamey","kano",400,360,"ne-ng-border","Frontière Niger – Nigeria",18000,75, "Maradi", "Jibia", "Katsina").ConfigureLocalities(2, "NE", "NE", "NG"),
        new RoadSegment("lagos","douala",1700,1560,"ng-cm-border","Frontière Nigeria – Cameroun",25000,120, "Benin City", "Enugu", "Calabar", "Kumba", "Buea").ConfigureLocalities(3, "NG", "NG", "NG", "CM", "CM"),
        new RoadSegment("douala","yaounde",245,210, null, null, 0L, 0, "Edea", "Mbankomo"),
        new RoadSegment("yaounde","libreville",680,630,"cm-ga-border","Frontière Cameroun – Gabon",22000,100, "Ebolowa", "Ambam")
    };

    public static IReadOnlyList<City> AllCities { get { return Cities; } }

    public static City FindCity(string cityId)
    {
        if (string.IsNullOrWhiteSpace(cityId)) return null;
        for (int i = 0; i < Cities.Length; i++)
            if (string.Equals(Cities[i].Id, cityId, StringComparison.OrdinalIgnoreCase)) return Cities[i];
        return null;
    }

    public static List<City> GetDirectConnections(string cityId)
    {
        List<City> result = new List<City>();
        for (int i = 0; i < Segments.Length; i++)
        {
            string next = null;
            if (Segments[i].FromCityId == cityId) next = Segments[i].ToCityId;
            else if (Segments[i].ToCityId == cityId) next = Segments[i].FromCityId;
            if (next != null)
            {
                City city = FindCity(next);
                if (city != null && !result.Exists(c => c.Id == city.Id)) result.Add(city);
            }
        }
        return result;
    }

    /// <summary>Builds exactly the sequence selected by the player, including chosen stopovers.</summary>
    public static bool TryBuildPlayerItinerary(IList<string> orderedCityIds, out RouteItinerary itinerary, out string reason)
    {
        itinerary = null;
        reason = string.Empty;
        if (orderedCityIds == null || orderedCityIds.Count < 2)
        { reason = "Choisissez au moins une ville de départ et une destination."; return false; }

        List<City> selected = new List<City>();
        HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < orderedCityIds.Count; i++)
        {
            City city = FindCity(orderedCityIds[i]);
            if (city == null) { reason = "Une ville choisie n'existe pas dans le réseau."; return false; }
            if (!unique.Add(city.Id)) { reason = "Une même ville ne peut pas être ajoutée deux fois à cet itinéraire."; return false; }
            selected.Add(city);
        }

        int totalKm = 0;
        int totalMinutes = 0;
        List<RouteStop> stops = new List<RouteStop>();
        List<BorderCheckpoint> borders = new List<BorderCheckpoint>();
        stops.Add(new RouteStop("player-origin-" + selected[0].Id, "Gare de " + selected[0].Name,
            selected[0].CountryCode, RouteStopType.Terminal, 20, true));

        for (int i = 0; i < selected.Count - 1; i++)
        {
            RoadSegment segment = FindSegment(selected[i].Id, selected[i + 1].Id);
            if (segment == null)
            { reason = "Aucune liaison routière disponible entre " + selected[i].Name + " et " + selected[i + 1].Name + ". Choisissez une autre ville intermédiaire."; return false; }

            totalKm += segment.DistanceKm;
            totalMinutes += segment.DrivingMinutes;
            // Only localities on the selected road segment are added. Reverse their order
            // when the player travels in the opposite direction.
            string[] localities = segment.IntermediateLocalities ?? new string[0];
            string[] localityCountries = segment.IntermediateLocalityCountryCodes ?? new string[0];
            bool forward = segment.FromCityId == selected[i].Id;
            int borderPosition = segment.BorderAfterLocalityCount;
            if (!forward) borderPosition = localities.Length - borderPosition;
            bool borderAdded = false;
            for (int step = 0; step <= localities.Length; step++)
            {
                if (!borderAdded && !string.IsNullOrEmpty(segment.BorderId) && step == borderPosition)
                {
                    string fromCountry = forward ? selected[i].CountryCode : selected[i + 1].CountryCode;
                    string toCountry = forward ? selected[i + 1].CountryCode : selected[i].CountryCode;
                    stops.Add(new RouteStop("border-stop-" + segment.BorderId, segment.BorderName,
                        toCountry, RouteStopType.BorderCheckpoint, 0, false));
                    borders.Add(new BorderCheckpoint(segment.BorderId, segment.BorderName,
                        fromCountry, toCountry, segment.BorderFeeXof, segment.BorderMinutes));
                    borderAdded = true;
                }
                if (step == localities.Length) break;
                int localityIndex = forward ? step : localities.Length - 1 - step;
                string localityCountry = localityIndex < localityCountries.Length &&
                    !string.IsNullOrWhiteSpace(localityCountries[localityIndex])
                    ? localityCountries[localityIndex] : selected[i].CountryCode;
                stops.Add(new RouteStop("locality-" + selected[i].Id + "-" + selected[i + 1].Id + "-" + localityIndex,
                    localities[localityIndex], localityCountry, RouteStopType.PassengerStop, 5, true));
            }
            bool finalCity = i == selected.Count - 2;
            stops.Add(new RouteStop("player-stop-" + selected[i + 1].Id,
                finalCity ? "Gare de " + selected[i + 1].Name : selected[i + 1].Name,
                selected[i + 1].CountryCode,
                finalCity ? RouteStopType.Terminal : RouteStopType.PassengerStop,
                20, true));
        }

        string displayName = selected[0].Name + " – " + selected[selected.Count - 1].Name;
        itinerary = new RouteItinerary("custom-" + string.Join("-", orderedCityIds),
            displayName, totalKm, totalMinutes, stops, borders);
        return true;
    }

    private static RoadSegment FindSegment(string cityA, string cityB)
    {
        for (int i = 0; i < Segments.Length; i++)
        {
            RoadSegment segment = Segments[i];
            if ((segment.FromCityId == cityA && segment.ToCityId == cityB) ||
                (segment.FromCityId == cityB && segment.ToCityId == cityA)) return segment;
        }
        return null;
    }
}
