using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Appointments;

/// <summary>
/// Domain event raised when an <see cref="Appointment"/> transitions to the
/// <see cref="AppointmentStatus.Cancelled"/> state via <c>Appointment.Cancel()</c>.
/// </summary>
/// <remarks>
/// This event is captured by <see cref="Infrastructure.OutboxMessages.InsertOutboxMessagesInterceptors"/>
/// and stored as an <see cref="Infrastructure.OutboxMessages.OutboxMessage"/> row.
/// The <see cref="Infrastructure.OutboxMessages.ProcessOutboxMessagesJob"/> then dispatches it
/// to all registered <c>IDomainEventHandler&lt;AppointmentCancelledDomainEvent&gt;</c> implementations,
/// such as the notification handler that sends a cancellation alert to the client.
/// </remarks>
/// <param name="AppointmentId">The unique identifier of the cancelled appointment.</param>
public sealed record AppointmentCancelledDomainEvent(Guid AppointmentId)
    : DomainEvent();
