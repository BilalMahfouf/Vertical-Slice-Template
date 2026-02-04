using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public sealed class AppointmentCancelledDomainEventHandler
    : IDomainEventHandler<AppointmentCancelledDomainEvent>
{
    private readonly IApplicationDbContext _db;

    public AppointmentCancelledDomainEventHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(
        AppointmentCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var data = await _db.Appointments
            .Where(e => e.Id == domainEvent.AppointmentId)
            .Select(e => new
            {
                ClientName = e.Animal.Client.FullName,
                CreatedOnUtc = e.CreatedOnUtc,

            }).FirstOrDefaultAsync(cancellationToken);
        if(data is null)
        {
            return;
        }

    }
}