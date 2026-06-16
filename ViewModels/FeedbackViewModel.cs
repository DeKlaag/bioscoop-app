using System.Security.Claims;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class FeedbackViewModel : ObservableObject
{
    private readonly IFeedbackService _feedbackService;
    private readonly IUserSession _session;

    public IReadOnlyList<string> Categories { get; } =
        ["Algemeen", "App", "Reserveringen", "Bioscoop", "Overig"];

    public IReadOnlyList<int> Ratings { get; } = [1, 2, 3, 4, 5];

    [ObservableProperty]
    private int _rating = 5;

    [ObservableProperty]
    private string? _selectedCategory = "Algemeen";

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public FeedbackViewModel(IFeedbackService feedbackService, IUserSession session)
    {
        _feedbackService = feedbackService;
        _session = session;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Message))
        {
            StatusMessage = "Vul eerst een bericht in.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var request = new FeedbackRequest
            {
                Email = _session.User?.FindFirst(ClaimTypes.Email)?.Value
                        ?? _session.User?.FindFirst("email")?.Value,
                Rating = Rating,
                Category = SelectedCategory,
                Message = Message!.Trim(),
            };

            var success = await _feedbackService.SubmitAsync(request);
            if (success)
            {
                StatusMessage = "Bedankt voor je feedback!";
                Message = string.Empty;
                Rating = 5;
                SelectedCategory = "Algemeen";
            }
            else
            {
                StatusMessage = "Versturen mislukt. Probeer het later opnieuw.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
