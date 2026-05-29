using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;

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
}
