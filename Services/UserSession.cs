using System.Security.Claims;

namespace bioscoop_app.Services;

public interface IUserSession
{
    ClaimsPrincipal? User { get; set; }
}

public class UserSession : IUserSession
{
    public ClaimsPrincipal? User { get; set; }
}