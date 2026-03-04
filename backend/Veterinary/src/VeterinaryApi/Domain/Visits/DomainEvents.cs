using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Visits;

/// <summary>
/// Domain event raised by the daily reminder job when a visit has an outstanding
/// (pending) payment that the clinic has not yet collected from the client.
/// </summary>
/// <param name="VisitId">The identifier of the visit with the pending payment.</param>
public sealed record VisitPaymentPendingReminderDomainEvent(Guid VisitId) : DomainEvent();


