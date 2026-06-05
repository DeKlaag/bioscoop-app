using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IPaymentService
{
    // Reserve the chosen seats and obtain a PrintCode for the group.
    // userEmail is the logged-in customer's email (null when reserving anonymously).
    Task<ReservationGroupResponse?> CreateReservationAsync(
        Guid screeningId, IEnumerable<(Guid SeatId, Guid TariffId)> tickets, string? userEmail);

    // Create a Stripe Checkout session for a reservation and return its hosted URL.
    Task<string?> CreateCheckoutUrlAsync(string printCode, Guid screeningId);

    // Confirm a completed Stripe payment by its session id; returns the PrintCode.
    Task<string?> ConfirmAsync(string sessionId);
}
