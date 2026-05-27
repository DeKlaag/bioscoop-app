# Connecting bioscoop-app to a Kotlin (Ktor) REST API with MVVM

This document explains how the Movies feature in this app is wired up — from the XAML on screen down to the HTTP call that hits your Ktor backend at `http://localhost:5033/api/movies/`. The pattern is adapted from `pjfmast/fsn-maui-TodoREST` (branch `use-mvvm`).

---

## 1. The mental model

```
┌─────────────────────────────┐
│  View  (MoviesPage.xaml)    │  ← what the user sees, only XAML
└──────────────┬──────────────┘
               │ bindings ({Binding Movies}, {Binding LoadCommand})
               ▼
┌─────────────────────────────┐
│  ViewModel                  │  ← state + commands, no UI types, no HTTP
│  (MoviesViewModel.cs)       │
└──────────────┬──────────────┘
               │ depends on IMovieService (interface)
               ▼
┌─────────────────────────────┐
│  Service (MovieService.cs)  │  ← owns HttpClient + JSON
└──────────────┬──────────────┘
               │ HTTP GET
               ▼
       Kotlin Ktor backend
   http://localhost:5033/api/movies/
```

Each layer only knows about the one below it through an interface. That separation is what makes the code testable, what stops "spaghetti code-behind", and what lets you swap the backend (or mock it for tests) without touching the UI.

---

## 2. The four layers, file by file

### Layer 1 — Model: `Models/MovieModel.cs`

A plain C# class whose shape matches the JSON the Ktor endpoint returns. No attributes, no logic.

```csharp
public class MovieModel
{
    public Guid ID { get; set; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
}
```

> **JSON mapping note**: Ktor's `kotlinx.serialization` emits camelCase by default (`id`, `title`, `imageUrl`). C# property names use PascalCase. The `MovieService` configures `JsonNamingPolicy.CamelCase` **and** `PropertyNameCaseInsensitive = true`, so both casings match. If Ktor emits something exotic (`movie_title`), add `[JsonPropertyName("movie_title")]` to the C# property.

### Layer 2 — Service: `Services/IMovieService.cs` + `Services/MovieService.cs`

The interface is the contract the ViewModel depends on:

```csharp
public interface IMovieService
{
    Task<List<MovieModel>> GetMoviesAsync();
}
```

The implementation owns the `HttpClient` and the JSON options. The ViewModel never sees `HttpClient` — that's the whole point.

```csharp
public class MovieService : IMovieService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovieService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<List<MovieModel>> GetMoviesAsync()
    {
        using var response = await _http.GetAsync(Constants.MoviesUrl);
        if (!response.IsSuccessStatusCode) return [];
        await using var stream = await response.Content.ReadAsStreamAsync();
        var movies = await JsonSerializer.DeserializeAsync<List<MovieModel>>(stream, _jsonOptions);
        return movies ?? [];
    }
}
```

Why one `HttpClient` for the whole app? Because creating new `HttpClient`s repeatedly leaks sockets on every platform. That's why `MovieService` is registered as a **singleton** in DI.

### Layer 3 — ViewModel: `ViewModels/MoviesViewModel.cs`

This is the brain of the screen. It holds the state the UI binds to (`Movies`, `IsBusy`, `ErrorMessage`) and the commands the UI can call (`LoadCommand`).

```csharp
public partial class MoviesViewModel : ObservableObject
{
    private readonly IMovieService _movieService;

    public ObservableCollection<MovieModel> Movies { get; } = [];

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public MoviesViewModel(IMovieService movieService) => _movieService = movieService;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true; ErrorMessage = null;
            var movies = await _movieService.GetMoviesAsync();
            Movies.Clear();
            foreach (var m in movies) Movies.Add(m);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
```

What's happening here?

- **`partial class`** — required because `CommunityToolkit.Mvvm` uses a Roslyn **source generator**. At compile time, it generates a second part of the same class with the boilerplate filled in.
- **`ObservableObject`** — base class that implements `INotifyPropertyChanged`. When you set `IsBusy = true`, it raises an event the UI listens for.
- **`[ObservableProperty] private bool _isBusy;`** — the generator turns this into a real public `IsBusy` property with a setter that calls `SetProperty(...)` and raises change notifications. You write 1 line; you get ~10 lines for free.
- **`[RelayCommand] private async Task LoadAsync()`** — the generator creates a public `LoadCommand` property of type `IAsyncRelayCommand` that the XAML binds to. It strips the `Async` suffix and adds `Command` (a Toolkit convention).
- **`ObservableCollection<T>`** — a list that raises events when items are added/removed/cleared. That's why `Movies.Clear()` followed by `Movies.Add(m)` makes the `CollectionView` redraw — the collection itself tells the UI it changed.
- **No `Microsoft.Maui` imports anywhere.** A ViewModel should never know whether it's running inside MAUI, WPF, or a unit test. That's the discipline.

### Layer 4 — View: `Views/MoviesPage.xaml` + `MoviesPage.xaml.cs`

The XAML is declarative. It binds to property and command names on its `BindingContext` (which is the ViewModel).

```xml
<ContentPage ...
             xmlns:vm="clr-namespace:bioscoop_app.ViewModels"
             xmlns:models="clr-namespace:bioscoop_app.Models"
             x:DataType="vm:MoviesViewModel"
             Title="Movies">
    <Grid RowDefinitions="Auto,*" Padding="12">
        <Label Grid.Row="0" Text="{Binding ErrorMessage}" TextColor="Red" />
        <RefreshView Grid.Row="1"
                     IsRefreshing="{Binding IsBusy}"
                     Command="{Binding LoadCommand}">
            <CollectionView ItemsSource="{Binding Movies}">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="models:MovieModel">
                        <Grid ColumnDefinitions="80,*" ColumnSpacing="12">
                            <Image Source="{Binding ImageUrl}" ... />
                            <Label Text="{Binding Title}" .../>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </RefreshView>
    </Grid>
</ContentPage>
```

Things to notice:

- **`x:DataType="vm:MoviesViewModel"`** — turns on **compiled bindings**. The XAML compiler verifies every `{Binding X}` against the named type at build time. A typo becomes a build error, not a silent runtime no-op. The inner `x:DataType="models:MovieModel"` switches the type inside the `DataTemplate` so each row binds against a single movie.
- **`IsRefreshing="{Binding IsBusy}"`** — two-way out of the box for `RefreshView`. Pulling down sets it `true`; the ViewModel setting it `false` ends the spinner.
- **`Command="{Binding LoadCommand}"`** — the pull triggers `LoadAsync` on the ViewModel. No `Clicked="..."` handler anywhere.
- **No business logic in `MoviesPage.xaml.cs`** — only `InitializeComponent()`, `BindingContext = vm;`, and an `OnAppearing` that kicks the first load. That's the discipline of "thin code-behind".

```csharp
public partial class MoviesPage : ContentPage
{
    private readonly MoviesViewModel _vm;
    public MoviesPage(MoviesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Movies.Count == 0)
            await _vm.LoadCommand.ExecuteAsync(null);
    }
}
```

### Glue — `MauiProgram.cs` (DI)

This is what allows `MoviesPage` to receive a `MoviesViewModel` in its constructor, and `MoviesViewModel` to receive an `IMovieService`. Nothing is `new`'d up by hand:

```csharp
builder.Services.AddSingleton<IMovieService, MovieService>();
builder.Services.AddSingleton<MoviesViewModel>();
builder.Services.AddSingleton<MoviesPage>();
```

Singleton vs Transient rule of thumb:

- **Singleton** for services (one `HttpClient`) and for list pages where you want the data cached across tab switches.
- **Transient** for detail/edit pages and their ViewModels — a fresh instance each time the user opens "edit movie #42".

---

## 3. Cross-platform tricks worth understanding

### Base URL: `Constants.cs`

```csharp
public static string LocalhostUrl =
    DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
public static string Scheme = "http";
public static string Port   = "5033";
public static string MoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/";
```

- **Android emulator** can't see the host machine's `localhost` — it gets its own loopback. `10.0.2.2` is a special alias the emulator routes back to the dev machine.
- **iOS simulator** shares the host's network stack, so `localhost` just works.
- **Real device** can't reach either name — you'd put your machine's LAN IP (e.g. `192.168.1.42`) here and make sure the Kotlin server binds to `0.0.0.0` instead of `127.0.0.1`.

### Cleartext HTTP on Android: `Platforms/Android/AndroidManifest.xml`

Because Ktor in dev is `http://` (not `https://`), Android blocks the call by default. The fix is one attribute on the `<application>` element:

```xml
<application ... android:usesCleartextTraffic="true"></application>
```

This is the single most common "it works in Postman but not in MAUI" cause. Production should use HTTPS and remove the flag.

### Ktor: binding to all interfaces

If you ever test from a real device, make sure your Ktor server is started like:

```kotlin
embeddedServer(Netty, port = 5033, host = "0.0.0.0") { ... }
```

Default `localhost`/`127.0.0.1` won't be reachable from the emulator's `10.0.2.2` alias on some setups.

---

## 4. End-to-end data flow (one round trip)

1. User taps the **Movies** tab → Shell instantiates `MoviesPage` via DI.
2. DI sees `MoviesPage` needs a `MoviesViewModel` → constructs one (or reuses the singleton).
3. DI sees `MoviesViewModel` needs an `IMovieService` → injects the singleton `MovieService`.
4. `MoviesPage.OnAppearing` runs → calls `LoadCommand.ExecuteAsync(null)`.
5. `LoadAsync` sets `IsBusy = true` → the `RefreshView` spinner appears (binding fires).
6. `LoadAsync` awaits `_movieService.GetMoviesAsync()`.
7. `MovieService` does `HttpClient.GetAsync("http://10.0.2.2:5033/api/movies/")`.
8. Response body streams into `JsonSerializer.DeserializeAsync<List<MovieModel>>(...)`.
9. List returns to the ViewModel → `Movies.Clear(); foreach (...) Movies.Add(m);`
10. `ObservableCollection` raises `CollectionChanged` → `CollectionView` redraws each item template.
11. `finally { IsBusy = false; }` → spinner disappears.

---

## 5. Common pitfalls and fixes

| Symptom | Likely cause | Fix |
|---|---|---|
| Empty list, no error | Android blocking cleartext HTTP | `usesCleartextTraffic="true"` in `AndroidManifest.xml` |
| `HttpRequestException: Connection refused` | Wrong host for the platform | Confirm `10.0.2.2` on Android, `localhost` on iOS |
| List loads but properties are `null` | JSON casing mismatch | Keep `PropertyNameCaseInsensitive = true` or add `[JsonPropertyName(...)]` |
| `CannotResolveType` build error on XAML | Compiled bindings can't find the type | Check `xmlns:vm` namespace and `x:DataType` value |
| `LoadCommand` is null in XAML | Forgot `partial` keyword on the ViewModel | Add `partial` so the source generator can extend it |
| New ViewModel doesn't fire any updates | Set the field (`_isBusy`) instead of the generated property (`IsBusy`) | Always set the public `IsBusy`; the field is private |
| Page crashes opening | Missed a DI registration in `MauiProgram.cs` | Register the page, the ViewModel, and the service |

---

## 6. Where to grow next

- **Detail view**: `MovieDetailPage` + `MovieDetailViewModel` registered as **transient**. Pass the movie id via Shell route parameters (`await Shell.Current.GoToAsync($"movie?id={movie.ID}")`) and decorate the VM with `[QueryProperty(nameof(MovieId), "id")]`.
- **POST/PUT/DELETE**: extend `IMovieService` with `SaveMovieAsync` / `DeleteMovieAsync`; mirror the reference repo's `SaveTodoItemAsync` for the `HttpClient.PostAsync` / `PutAsync` / `DeleteAsync` pattern.
- **Auth0 token forwarding**: store the bearer token after `LoginAsync` (e.g. in `SecureStorage`), and add `_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);` in `MovieService` so the Kotlin backend can validate the JWT.
- **Migrate `MainPage` to MVVM**: create a `MainViewModel` with `LoginCommand` / `LogoutCommand` and `[ObservableProperty]` for `IsLoggedIn`, `UserName`, `PictureUrl`. The XAML then uses `IsVisible="{Binding IsLoggedIn}"` instead of code-behind toggling `LoginView.IsVisible = false;`.
- **Unit testing**: because the ViewModel depends on `IMovieService`, you can write a fake `IMovieService` that returns canned data and test `MoviesViewModel.LoadAsync` without spinning up MAUI or the backend.
