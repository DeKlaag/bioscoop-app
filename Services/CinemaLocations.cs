namespace bioscoop_app.Services;

/// <summary>A Pathé cinema with its approximate geographic location.</summary>
public readonly record struct CinemaLocation(string Name, string Address, double Lat, double Lng);

/// <summary>
/// Single source of truth for the Pathé cinemas in the Netherlands (locatie bij benadering).
/// Used both for the map pins and for the foreground proximity check.
/// </summary>
public static class CinemaLocations
{
    public static readonly IReadOnlyList<CinemaLocation> All = new[]
    {
        new CinemaLocation("Pathé Tuschinski", "Reguliersbreestraat 26-34, Amsterdam", 52.3666, 4.8957),
        new CinemaLocation("Pathé De Munt", "Vijzelstraat 15, Amsterdam", 52.3663, 4.8914),
        new CinemaLocation("Pathé City", "Kleine-Gartmanplantsoen 15-25, Amsterdam", 52.3640, 4.8814),
        new CinemaLocation("Pathé Arena", "ArenA Boulevard 600, Amsterdam", 52.3127, 4.9413),
        new CinemaLocation("Pathé Amsterdam Noord", "Buikslotermeerplein 101, Amsterdam", 52.3940, 4.9416),
        new CinemaLocation("Pathé Amersfoort", "Eemplein 71, Amersfoort", 52.1623, 5.3756),
        new CinemaLocation("Pathé Arnhem", "Onderlangs 1, Arnhem", 51.9760, 5.8980),
        new CinemaLocation("Pathé Breda", "Frankenthalerstraat 5, Breda", 51.5719, 4.7683),
        new CinemaLocation("Pathé Delft", "Vesteplein 100, Delft", 52.0086, 4.3640),
        new CinemaLocation("Pathé Spuimarkt", "Spui 191, Den Haag", 52.0760, 4.3160),
        new CinemaLocation("Pathé Buitenhof", "Buitenhof 20, Den Haag", 52.0796, 4.3108),
        new CinemaLocation("Pathé Ypenburg", "Laan van Hofrust 10, Den Haag", 52.0473, 4.3690),
        new CinemaLocation("Pathé Scheveningen", "Gevers Deynootweg 990, Den Haag", 52.1090, 4.2740),
        new CinemaLocation("Pathé Ede", "Maandereind 35, Ede", 52.0440, 5.6680),
        new CinemaLocation("Pathé Eindhoven", "Dommelstraat 27, Eindhoven", 51.4400, 5.4830),
        new CinemaLocation("Pathé Groningen", "Gedempte Zuiderdiep 78, Groningen", 53.2150, 6.5670),
        new CinemaLocation("Pathé Haarlem", "Gedempte Oude Gracht 60, Haarlem", 52.3790, 4.6370),
        new CinemaLocation("Pathé Helmond", "Engelseweg 215, Helmond", 51.4790, 5.6570),
        new CinemaLocation("Pathé Leeuwarden", "Ruiterskwartier 41, Leeuwarden", 53.2010, 5.7980),
        new CinemaLocation("Pathé Leidschendam", "Damplein 90, Leidschendam", 52.0840, 4.3940),
        new CinemaLocation("Pathé Maastricht", "Wilhelminasingel 39, Maastricht", 50.8480, 5.6960),
        new CinemaLocation("Pathé Nijmegen", "Mariënburg 28, Nijmegen", 51.8430, 5.8650),
        new CinemaLocation("Pathé Schouwburgplein", "Schouwburgplein 101, Rotterdam", 51.9230, 4.4720),
        new CinemaLocation("Pathé De Kuip", "Veranda 825, Rotterdam", 51.8930, 4.5230),
        new CinemaLocation("Pathé Tilburg", "Pieter Vreedeplein 70, Tilburg", 51.5570, 5.0890),
        new CinemaLocation("Pathé Utrecht", "Sint Jacobsstraat 16, Utrecht", 52.0950, 5.1120),
        new CinemaLocation("Pathé Rembrandt", "Oudegracht 73, Utrecht", 52.0900, 5.1180),
        new CinemaLocation("Pathé Zaandam", "Gedempte Gracht 1, Zaandam", 52.4390, 4.8260),
        new CinemaLocation("Pathé Zwolle", "Gasthuisplein 5, Zwolle", 52.5120, 6.0930),
    };
}
