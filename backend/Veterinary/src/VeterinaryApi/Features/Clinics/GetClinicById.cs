using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

public static class GetClinicById
{
    public record Query(Guid Id) : IQuery<Response>;
    public record Response(Guid Id, string Name, string Phone, string Address);

    public class GetClinicByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        public GetClinicByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var clinic = await _db.Clinics
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Phone,
                    e.Address))
                .FirstOrDefaultAsync(cancellationToken);
            if (clinic is null)
            {
                return Result<Response>
                    .Failure(ClinicErrors.ClinicNotFound(query.Id));
            }
            return Result<Response>.Success(clinic);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clinics/{id:guid}", async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            });
        }
    }
}
