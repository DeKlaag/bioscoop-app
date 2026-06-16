using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IOfferService
{
    /// <summary>Returns the current offers &amp; promotions bundled with the app.</summary>
    Task<List<OfferModel>> GetOffersAsync();
}
