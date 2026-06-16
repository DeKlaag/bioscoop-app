using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IFeedbackService
{
    /// <summary>Submits user feedback to the backend. Returns true on success.</summary>
    Task<bool> SubmitAsync(FeedbackRequest request);
}
