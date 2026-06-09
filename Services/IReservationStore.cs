using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IReservationStore
{
    List<ReservationModel> Load(string email);
    void Save(string email, IReadOnlyList<ReservationModel> reservations);
}
