using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class MoviesViewModel : ObservableObject
{
    private readonly IMovieService _movieService;
    private readonly List<MovieModel> _allMovies = [];

    public ObservableCollection<MovieModel> Movies { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _searchText;

    partial void OnSearchTextChanged(string? value) => ApplyFilter();

    public MoviesViewModel(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var movies = await _movieService.GetMoviesAsync();

            _allMovies.Clear();
            _allMovies.AddRange(movies);
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

    private void ApplyFilter()
    {
        Movies.Clear();
        var query = SearchText?.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? _allMovies
            : _allMovies.Where(m => m.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true);
        foreach (var m in filtered)
            Movies.Add(m);
    }
}