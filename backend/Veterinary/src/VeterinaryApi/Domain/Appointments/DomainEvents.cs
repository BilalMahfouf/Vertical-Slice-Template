using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Appointments;


public sealed record AppointmentCancelledDomainEvent(Guid AppointmentId)
    : DomainEvent();
