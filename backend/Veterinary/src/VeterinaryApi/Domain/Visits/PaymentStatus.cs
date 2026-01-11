namespace VeterinaryApi.Domain.Visits;

public enum PaymentStatus : byte
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Refunded = 4
}
