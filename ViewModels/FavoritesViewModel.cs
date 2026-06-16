using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private const string AllGenres = "Alle genres";

    private readonly IMovieService _movieService;
    private readonly IFavoritesService _favoritesService;
    private readonly List<MovieModel> _allFavorites = [];

    public ObservableCollection<MovieModel> Movies { get; } = [];
    public ObservableCollection<string> Genres { get; } = [];
    public IReadOnlyList<string> SortOptions { get; } = ["Titel", "Genre"];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedGenre = AllGenres;

    [ObservableProperty]
    private string _sortOption = "Titel";

    partial void OnSelectedGenreChanged(string value) => ApplyFilter();
    partial void OnSortOptionChanged(string value) => ApplyFilter();

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

            _allFavorites.Clear();
            _allFavorites.AddRange(movies.Where(m => favoriteIds.Contains(m.ID)));

            RebuildGenres();
            ApplyFilter();
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

    private void RebuildGenres()
    {
        var genres = _allFavorites
            .Where(m => !string.IsNullOrWhiteSpace(m.Genre))
            .Select(m => m.Genre!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g);

        Genres.Clear();
        Genres.Add(AllGenres);
        foreach (var genre in genres)
            Genres.Add(genre);

        if (!Genres.Contains(SelectedGenre))
            SelectedGenre = AllGenres;
    }

    private void ApplyFilter()
    {
        IEnumerable<MovieModel> filtered = _allFavorites;

        if (!string.IsNullOrEmpty(SelectedGenre) && SelectedGenre != AllGenres)
            filtered = filtered.Where(m =>
                string.Equals(m.Genre?.Trim(), SelectedGenre, StringComparison.OrdinalIgnoreCase));

        filtered = SortOption == "Genre"
            ? filtered.OrderBy(m => m.Genre).ThenBy(m => m.Title)
            : filtered.OrderBy(m => m.Title);

        Movies.Clear();
        foreach (var movie in filtered)
            Movies.Add(movie);
    }
}
