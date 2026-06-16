using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class FeedbackPage : ContentPage
{
    public FeedbackPage(FeedbackViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
