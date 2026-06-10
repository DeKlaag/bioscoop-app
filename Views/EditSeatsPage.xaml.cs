using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class EditSeatsPage : ContentPage
{
    public EditSeatsPage(EditSeatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
