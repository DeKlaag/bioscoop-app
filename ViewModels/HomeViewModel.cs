using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

// Backs the personalized Home page. Unlike MoviesViewModel (still used by MoviesPage),
// this also loads recommendations and offers so Home can show curated sections above
// the full movie list.
public partial class HomeViewModel : ObservableObject
{
    private readonly IMovieService _movieService;
    private readonly IRecommendationService _recommendationService;
    private readonly IOfferService _offerService;
    private readonly List<MovieModel> _allMovies = [];

    public ObservableCollection<MovieModel> Recommended { get; } = [];
    public ObservableCollection<OfferModel> Offers { get; } = [];
    public ObservableCollection<MovieModel> Movies { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private bool _hasRecommendations;

    [ObservableProperty]
    private bool _hasOffers;

    partial void OnSearchTextChanged(string? value) => ApplyFilter();

    public HomeViewModel(
        IMovieService movieService,
        IRecommendationService recommendationService,
        IOfferService offerService)
    {
        _movieService = movieService;
        _recommendationService = recommendationService;
        _offerService = offerService;
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

            Recommended.Clear();
            foreach (var movie in await _recommendationService.GetRecommendationsAsync())
                Recommended.Add(movie);
            HasRecommendations = Recommended.Count > 0;

            Offers.Clear();
            foreach (var offer in await _offerService.GetOffersAsync())
                Offers.Add(offer);
            HasOffers = Offers.Count > 0;
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
