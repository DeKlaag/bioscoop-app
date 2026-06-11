using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using AppMap = Microsoft.Maui.ApplicationModel.Map;

namespace bioscoop_app.Views;

public partial class MapPage : ContentPage
{
    // Pathé bioscopen in Nederland (locatie bij benadering).
    private static readonly (string Name, string Address, double Lat, double Lng)[] PatheCinemas =
    {
        ("Pathé Tuschinski", "Reguliersbreestraat 26-34, Amsterdam", 52.3666, 4.8957),
        ("Pathé De Munt", "Vijzelstraat 15, Amsterdam", 52.3663, 4.8914),
        ("Pathé City", "Kleine-Gartmanplantsoen 15-25, Amsterdam", 52.3640, 4.8814),
        ("Pathé Arena", "ArenA Boulevard 600, Amsterdam", 52.3127, 4.9413),
        ("Pathé Amsterdam Noord", "Buikslotermeerplein 101, Amsterdam", 52.3940, 4.9416),
        ("Pathé Amersfoort", "Eemplein 71, Amersfoort", 52.1623, 5.3756),
        ("Pathé Arnhem", "Onderlangs 1, Arnhem", 51.9760, 5.8980),
        ("Pathé Breda", "Frankenthalerstraat 5, Breda", 51.5719, 4.7683),
        ("Pathé Delft", "Vesteplein 100, Delft", 52.0086, 4.3640),
        ("Pathé Spuimarkt", "Spui 191, Den Haag", 52.0760, 4.3160),
        ("Pathé Buitenhof", "Buitenhof 20, Den Haag", 52.0796, 4.3108),
        ("Pathé Ypenburg", "Laan van Hofrust 10, Den Haag", 52.0473, 4.3690),
        ("Pathé Scheveningen", "Gevers Deynootweg 990, Den Haag", 52.1090, 4.2740),
        ("Pathé Ede", "Maandereind 35, Ede", 52.0440, 5.6680),
        ("Pathé Eindhoven", "Dommelstraat 27, Eindhoven", 51.4400, 5.4830),
        ("Pathé Groningen", "Gedempte Zuiderdiep 78, Groningen", 53.2150, 6.5670),
        ("Pathé Haarlem", "Gedempte Oude Gracht 60, Haarlem", 52.3790, 4.6370),
        ("Pathé Helmond", "Engelseweg 215, Helmond", 51.4790, 5.6570),
        ("Pathé Leeuwarden", "Ruiterskwartier 41, Leeuwarden", 53.2010, 5.7980),
        ("Pathé Leidschendam", "Damplein 90, Leidschendam", 52.0840, 4.3940),
        ("Pathé Maastricht", "Wilhelminasingel 39, Maastricht", 50.8480, 5.6960),
        ("Pathé Nijmegen", "Mariënburg 28, Nijmegen", 51.8430, 5.8650),
        ("Pathé Schouwburgplein", "Schouwburgplein 101, Rotterdam", 51.9230, 4.4720),
        ("Pathé De Kuip", "Veranda 825, Rotterdam", 51.8930, 4.5230),
        ("Pathé Tilburg", "Pieter Vreedeplein 70, Tilburg", 51.5570, 5.0890),
        ("Pathé Utrecht", "Sint Jacobsstraat 16, Utrecht", 52.0950, 5.1120),
        ("Pathé Rembrandt", "Oudegracht 73, Utrecht", 52.0900, 5.1180),
        ("Pathé Zaandam", "Gedempte Gracht 1, Zaandam", 52.4390, 4.8260),
        ("Pathé Zwolle", "Gasthuisplein 5, Zwolle", 52.5120, 6.0930),
    };

    public MapPage()
    {
        InitializeComponent();
        AddCinemaPins();
    }

    private void AddCinemaPins()
    {
        foreach (var cinema in PatheCinemas)
        {
            var pin = new Pin
            {
                Label = cinema.Name,
                Address = cinema.Address,
                Type = PinType.Place,
                Location = new Location(cinema.Lat, cinema.Lng),
            };
            pin.InfoWindowClicked += OnCinemaInfoWindowClicked;
            map.Pins.Add(pin);
        }
    }

    private async void OnCinemaInfoWindowClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin || pin.Location is null)
        {
            return;
        }

        e.HideInfoWindow = true;

        var options = new MapLaunchOptions
        {
            Name = pin.Label,
            NavigationMode = NavigationMode.Driving,
        };

        try
        {
            await AppMap.OpenAsync(pin.Location, options);
        }
        catch (Exception)
        {
            await DisplayAlertAsync(
                "Navigatie",
                "Kon de kaart-app niet openen voor de routebeschrijving.",
                "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RequestLocationPermissionAsync();
    }

    private async Task RequestLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status == PermissionStatus.Granted)
        {
            // Enable the blue user-location dot only after permission is granted,
            // otherwise the native iOS map crashes when it starts location updates.
            map.IsShowingUser = true;
            await MoveToUserLocationAsync();
        }
        else
        {
            await DisplayAlertAsync(
                "Locatie",
                "Zonder locatietoestemming kan je positie niet op de kaart worden getoond.",
                "OK");
        }
    }

    private async Task MoveToUserLocationAsync()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(
                               new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location is not null)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(location.Latitude, location.Longitude),
                    Distance.FromKilometers(2)));
            }
        }
        catch (Exception)
        {
            // Location lookup can fail (e.g. no GPS fix); leave the map at its default region.
        }
    }
}