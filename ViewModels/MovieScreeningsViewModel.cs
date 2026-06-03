using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(MovieId), "id")]
public partial class MovieScreeningsViewModel : ObservableObject
{
    private readonly IScreeningService _screeningService;

    [ObservableProperty]
    private string? _movieId;

    [ObservableProperty]
    private string? _movieTitle;

    public ObservableCollection<ScreeningModel> Screenings { get; } = new();

    public MovieScreeningsViewModel(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }

    partial void OnMovieIdChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var items = await _screeningService.GetScreeningsByMovieAsync(id);
        Screenings.Clear();
        foreach (var screening in items)
            Screenings.Add(screening);

        MovieTitle = items.FirstOrDefault()?.Movie?.Title;
    }

    [RelayCommand]
    private async Task SelectScreeningAsync(ScreeningModel? screening)
    {
        if (screening is null) return;
        await Shell.Current.GoToAsync(nameof(ScreeningPage),
            new Dictionary<string, object> { ["screening"] = screening });
    }
}
