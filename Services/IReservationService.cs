using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IReservationService
{
    Task<List<ReservationModel>> GetReservationsByEmailAsync(string email);
}
