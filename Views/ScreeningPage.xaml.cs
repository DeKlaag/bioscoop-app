using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class ScreeningPage : ContentPage
{
    public ScreeningPage(ScreeningViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
