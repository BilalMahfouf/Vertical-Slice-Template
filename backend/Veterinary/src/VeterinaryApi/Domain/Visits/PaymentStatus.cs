namespace VeterinaryApi.Domain.Visits;

/// <summary>
/// Represents the payment settlement state for a veterinary <see cref="Visit"/>.
/// </summary>
public enum PaymentStatus : byte
{
    /// <summary>Payment has not yet been received. Default state on visit creation.</summary>
    Pending = 1,

    /// <summary>The full visit fee has been paid.</summary>
    Paid = 2,

    /// <summary>A partial payment has been received; the remainder is still outstanding.</summary>
    PartiallyPaid = 3,

    /// <summary>The payment was refunded to the client.</summary>
    Refunded = 4
}
