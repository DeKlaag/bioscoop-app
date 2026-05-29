using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(MovieId), "id")]
public partial class MovieDetailViewModel : ObservableObject
{
    private readonly IMovieService _movieService;

    [ObservableProperty]
    private string? _movieId;

    [ObservableProperty]
    private MovieModel? _movie;

    public MovieDetailViewModel(IMovieService movieService)
    {
        _movieService = movieService;
    }

    partial void OnMovieIdChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        Movie = await _movieService.GetMovieByIdAsync(id);
    }

    [RelayCommand]
    private async Task GoToScreeningsAsync()
    {
        if (Movie is null) return;
        await Shell.Current.GoToAsync($"{nameof(MovieScreenings)}?id={Movie.ID}");
    }
}
