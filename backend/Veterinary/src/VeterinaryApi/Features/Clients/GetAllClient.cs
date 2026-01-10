using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

public static class GetAllClients
{

    public record Response(
        Guid Id,
        Guid ClinicId,
        string ClinicName,
        string FullName,
        string Phone,
        string? Notes,
        DateTime CreatedOnUtc,
        int NumberOfAnimals);

    public class GetAllClientsQueryHandler
        : IQueryHandler<TableRequest<Response>, PagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllClientsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Clients.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<PagedList<Response>>.Failure(
                    ClientErrors.ClientsNotFound);
            }
            var clients = _db.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                clients = clients.Where(e =>
                    e.FullName.ToLower().Contains(query.search) ||
                    e.Phone.ToLower().Contains(query.search) ||
                    e.Clinic.Name.ToLower().Contains(query.search));
            }
            var client = clients.AsNoTracking()
                            .Select(e => new Response(
                                e.Id,
                                e.ClinicId,
                                e.Clinic.Name,
                                e.FullName,
                                e.Phone,
                                e.Notes,
                                e.CreatedOnUtc,
                                e.Animals.Count));

            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                .ToLower() switch
            {
                "fullname" => e => e.FullName,
                "phone" => e => e.Phone,
                "clinicname" => e => e.ClinicName,
                _ => e => e.Id
            };
            var temp = await client.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<PagedList<Response>>
                    .Failure(ClientErrors.ClientsNotFound);
            }
            var clientQuery = temp.AsQueryable();
            if (query.SortOrder is "desc")
            {
                clientQuery = clientQuery.OrderByDescending(orderSelector);
            }
            else
            {
                clientQuery = clientQuery.OrderBy(orderSelector);
            }

            clientQuery = clientQuery.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = clientQuery.ToList();
            if (data is null)
            {
                return Result<PagedList<Response>>
                    .Failure(ClientErrors.ClientsNotFound);
            }
            var result = PagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<PagedList<Response>>.Success(result);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clients", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                [FromServices] IQueryHandler<TableRequest<Response>
                , PagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var request = TableRequest<Response>
                    .Create(pageSize, page, search, sortColumn, sortOrder);

                var result = await handler.Handle(request, cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();

            }).WithTags("clients");
        }
    }
}