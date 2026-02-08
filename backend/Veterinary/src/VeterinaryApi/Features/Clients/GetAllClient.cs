using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

public static class GetAllClients
{


    public class GetAllClientsQueryHandler
        : IQueryHandler<TableRequest<ClientReadResponse>, OffSetPagedList<ClientReadResponse>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllClientsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<OffSetPagedList<ClientReadResponse>>> Handle(
            TableRequest<ClientReadResponse> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Clients.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<ClientReadResponse>>.Failure(
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
                            .Select(e => new ClientReadResponse(
                                e.Id,
                                e.ClinicId,
                                e.Clinic.Name,
                                e.FullName,
                                e.Phone,
                                e.Notes,
                                e.CreatedOnUtc,
                                e.Animals.Count));

            Expression<Func<ClientReadResponse, object>> orderSelector = query.SortColumn?
                .ToLower() switch
            {
                "fullname" => e => e.FullName,
                "phone" => e => e.Phone,
                "clinicname" => e => e.ClinicName,
                "numberofanimals" => e => e.NumberOfAnimals,
                _ => e => e.CreatedOnUtc
            };
            var temp = await client.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<ClientReadResponse>>
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
                return Result<OffSetPagedList<ClientReadResponse>>
                    .Failure(ClientErrors.ClientsNotFound);
            }
            var result = OffSetPagedList<ClientReadResponse>
                .Create(data, count, query.Page, query.PageSize);
            return Result<OffSetPagedList<ClientReadResponse>>.Success(result);
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
                [FromServices] IQueryHandler<TableRequest<ClientReadResponse>
                , OffSetPagedList<ClientReadResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var request = TableRequest<ClientReadResponse>
                    .Create(pageSize, page, search, sortColumn, sortOrder);

                var result = await handler.Handle(request, cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();

            })
            .WithTags($"{nameof(Client)}s")
            .WithSummary("Get all clients")
            .WithDescription("Retrieves a paginated list of all clients with optional search, sorting, and filtering capabilities. Includes animal count per client.");
        }
    }
}