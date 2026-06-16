using bioscoop_app.ViewModels;

namespace bioscoop_app;

public partial class MainPage : ContentPage
{
    private readonly HomeViewModel _vm;

    public MainPage(HomeViewModel homeViewModel)
    {
        InitializeComponent();
        BindingContext = _vm = homeViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
