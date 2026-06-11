using bioscoop_app.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using AppMap = Microsoft.Maui.ApplicationModel.Map;

namespace bioscoop_app.Views;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        AddCinemaPins();
    }

    private void AddCinemaPins()
    {
        foreach (var cinema in CinemaLocations.All)
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