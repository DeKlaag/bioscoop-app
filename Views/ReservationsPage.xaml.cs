namespace bioscoop_app.Views;

using bioscoop_app.ViewModels;

public partial class ReservationsPage : ContentPage
{
    private readonly ReservationsViewModel _vm;

    public ReservationsPage(ReservationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Reload each time the tab is shown so newly placed reservations appear.
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
