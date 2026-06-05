namespace bioscoop_app.Models;

// Returned by POST /api/reservations/website after the seats are reserved.
// The PrintCode is the key that links the reservation to the Stripe checkout.
public class ReservationGroupResponse
{
    public string PrintCode { get; set; } = string.Empty;
    public string? MovieTitle { get; set; }
    public string? Status { get; set; }
    public decimal TotalAmount { get; set; }
}

// Returned by POST /api/stripe/checkout: the hosted Stripe Checkout page URL.
public class CheckoutUrlResponse
{
    public string? Url { get; set; }
}

// Returned by POST /api/stripe/confirm once the payment is verified.
public class ConfirmResponse
{
    public string? PrintCode { get; set; }
}
