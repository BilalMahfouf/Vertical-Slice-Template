using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Notifications;

public static class GetAllNotifications
{
    public sealed record Response(
        Guid Id,
        string Title,
        string Body,
        DateTime CreatedOnUtc);

    public sealed class GetAllNotificationQueryHandler
        : IQueryHandler<TableRequest<Response>, PagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public GetAllNotificationQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        public async Task<Result<PagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Notifications
                .ForTenant(_currentTenant.UserId)
                .CountAsync();
            if (count <= 0)
            {
                return Result<PagedList<Response>>
                    .Failure(NotificationErrors.NotFound);
            }
            var notifications = await _db.Notifications
                .ForTenant(_currentTenant.UserId)
                .Where(e => e.IsRead == false)
                .Select(e => new Response(e.Id, e.Title, e.Body, e.CreatedOnUtc))
                .OrderByDescending(e => e.CreatedOnUtc)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
            if (notifications is null || !notifications.Any())
            {
                return Result<PagedList<Response>>
                    .Failure(NotificationErrors.NotFound);
            }
            var response = PagedList<Response>
                .Create(notifications, count, query.Page, query.PageSize);
            return Result<PagedList<Response>>.Success(response);
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/notifcations", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? search,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                IQueryHandler<TableRequest<Response>, PagedList<Response>> handler,
                CancellationToken ct = default) =>
            {
                var query = TableRequest<Response>.Create(
                    pageSize,
                    page,
                    search,
                    sortColumn,
                    sortOrder);
                var result = await handler.Handle(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();
            })
            .WithTags($"{nameof(Notification)}s")
            .WithSummary("Get all notifications")
            .WithDescription("Retrieves a paginated list of unread notifications for the current user, ordered by creation date (newest first).");
        }
    }
}
