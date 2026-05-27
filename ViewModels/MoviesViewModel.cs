using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class MoviesViewModel : ObservableObject
{
    private readonly IMovieService _movieService;

    public ObservableCollection<MovieModel> Movies { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

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

            Movies.Clear();
            foreach (var m in movies)
                Movies.Add(m);
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
}