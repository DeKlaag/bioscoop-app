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
    private readonly IFavoritesService _favoritesService;

    [ObservableProperty]
    private string? _movieId;

    [ObservableProperty]
    private MovieModel? _movie;

    [ObservableProperty]
    private bool _isFavorite;

    public string FavoriteButtonText => IsFavorite ? "★" : "☆";

    partial void OnIsFavoriteChanged(bool value) => OnPropertyChanged(nameof(FavoriteButtonText));

    public MovieDetailViewModel(IMovieService movieService, IFavoritesService favoritesService)
    {
        _movieService = movieService;
        _favoritesService = favoritesService;
    }

    partial void OnMovieIdChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
            _ = LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        Movie = await _movieService.GetMovieByIdAsync(id);
        IsFavorite = _favoritesService.IsFavorite(id);
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Movie is null) return;
        _favoritesService.Toggle(Movie.ID);
        IsFavorite = _favoritesService.IsFavorite(Movie.ID);
    }

    [RelayCommand]
    private async Task GoToScreeningsAsync()
    {
        if (Movie is null) return;
        await Shell.Current.GoToAsync($"{nameof(MovieScreenings)}?id={Movie.ID}");
    }
}
