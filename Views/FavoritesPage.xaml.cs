namespace bioscoop_app.Views;

using bioscoop_app.ViewModels;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _vm;

    public FavoritesPage(FavoritesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Reload each time the tab is shown so newly toggled favorites appear.
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
