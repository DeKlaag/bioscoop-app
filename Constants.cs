namespace bioscoop_app;

public static class Constants
{
    public static string LocalhostUrl = "192.168.178.107";
    // public static string LocalhostUrl = "192.168.178.129";
    public static string Scheme = "http";
    public static string Port = "5033";
    public static string UpcomingMoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/upcoming";
    public static string MoviesUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/movies/";
    public static string ScreeningsURL = $"{Scheme}://{LocalhostUrl}:{Port}/api/screenings/";
    public static string TariffsURL = $"{Scheme}://{LocalhostUrl}:{Port}/api/tariffs/";
    public static string UserReservationsURL = $"{Scheme}://{LocalhostUrl}:{Port}/api/Reservations/by-email/";

    // The API base URL, also reused as the FrontendBaseUrl sent to Stripe so the
    // checkout success/cancel redirects point back at a host the in-app WebView can intercept.
    public static string ApiBaseUrl = $"{Scheme}://{LocalhostUrl}:{Port}";
    public static string ReservationsWebsiteUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/reservations/website";
    public static string StripeCheckoutUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/stripe/checkout";
    public static string StripeConfirmUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/stripe/confirm";
    public static string FeedbackUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/feedback";

    public static string ScreeningSeatsUrl(Guid screeningId) =>
        $"{Scheme}://{LocalhostUrl}:{Port}/api/screenings/{screeningId}/seats";

    public static string ReservationByCodeUrl(string code) =>
        $"{Scheme}://{LocalhostUrl}:{Port}/api/reservations/by-code/{code}";

    public static string ReservationCancelUrl(string code) =>
        $"{Scheme}://{LocalhostUrl}:{Port}/api/reservations/{code}/cancel";

    public static string ReservationSeatsUrl(string code) =>
        $"{Scheme}://{LocalhostUrl}:{Port}/api/reservations/{code}/seats";

    public static string ReservationCheckInUrl(string code) =>
        $"{Scheme}://{LocalhostUrl}:{Port}/api/reservations/{code}/checkin";

    public static string UserReservationsUrl(string email) =>
        $"{UserReservationsURL}{Uri.EscapeDataString(email)}";
}