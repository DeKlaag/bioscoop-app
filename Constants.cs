namespace bioscoop_app;

public static class Constants
{
    public static string LocalhostUrl = "192.168.178.107";
    public static string Scheme = "http";
    public static string Port = "5033";
    public static string UpcomingMoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/upcoming";
    public static string MoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/";
    public static string ScreeningsURL = $"{Scheme}://{LocalhostUrl}:{Port}/api/screenings/";
}