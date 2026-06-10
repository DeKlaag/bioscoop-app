using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class ReservationDetailPage : ContentPage
{
    private readonly ReservationDetailViewModel _vm;

    public ReservationDetailPage(ReservationDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh on return (e.g. after editing seats) so the screen stays current.
        await _vm.ReloadAsync();
    }
}
