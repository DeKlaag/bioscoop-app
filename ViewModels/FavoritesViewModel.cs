using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly IMovieService _movieService;
    private readonly IFavoritesService _favoritesService;

    public ObservableCollection<MovieModel> Movies { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public FavoritesViewModel(IMovieService movieService, IFavoritesService favoritesService)
    {
        _movieService = movieService;
        _favoritesService = favoritesService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var favoriteIds = _favoritesService.GetFavoriteIds().ToHashSet();
            var movies = await _movieService.GetMoviesAsync();

            Movies.Clear();
            foreach (var movie in movies.Where(m => favoriteIds.Contains(m.ID)))
                Movies.Add(movie);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectMovieAsync(MovieModel? movie)
    {
        if (movie is null) return;
        await Shell.Current.GoToAsync($"{nameof(MovieDetailPage)}?id={movie.ID}");
    }
}
