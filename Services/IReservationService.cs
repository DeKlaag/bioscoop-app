using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IReservationService
{
    Task<List<ReservationModel>> GetReservationsByEmailAsync(string email);

    // Fetches a single reservation group by its print code (used to refresh the
    // detail screen after an update/cancel).
    Task<ReservationModel?> GetByCodeAsync(string code);

    // Soft-cancels the reservation group; returns the refreshed reservation.
    Task<ReservationModel?> CancelAsync(string code);

    // Replaces the reservation's seats with a new set of the same size.
    // Returns the refreshed reservation, or throws ReservationConflictException
    // when one of the chosen seats is no longer available.
    Task<ReservationModel?> UpdateSeatsAsync(string code, IReadOnlyList<Guid> seatIds);

    // Attempts to check a reservation in (staff scans the QR at the entrance).
    Task<CheckInResultModel?> CheckInAsync(string code);
}

// Thrown when the API rejects a seat change because a seat is already taken (409).
public class ReservationConflictException : Exception
{
    public ReservationConflictException(string message) : base(message) { }
}
