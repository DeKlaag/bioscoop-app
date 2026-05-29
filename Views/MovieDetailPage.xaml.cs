using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class MovieDetailPage : ContentPage
{
    public MovieDetailPage(MovieDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
