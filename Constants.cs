namespace bioscoop_app;

public static class Constants
{
    public static string LocalhostUrl =
        DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
    public static string Scheme = "http";
    public static string Port = "5033";
    public static string MoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/";
}