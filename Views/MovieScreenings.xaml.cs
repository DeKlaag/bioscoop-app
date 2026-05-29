using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class MovieScreenings : ContentPage
{
    public MovieScreenings(MovieScreeningsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
