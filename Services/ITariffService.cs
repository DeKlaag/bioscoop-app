using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface ITariffService
{
    Task<List<TariffModel>> GetTariffsAsync();
}
