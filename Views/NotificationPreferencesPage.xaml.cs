using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class NotificationPreferencesPage : ContentPage
{
    public NotificationPreferencesPage(NotificationPreferencesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
