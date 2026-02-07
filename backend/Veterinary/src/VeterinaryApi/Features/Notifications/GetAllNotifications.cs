using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.Cursor;
using VeterinaryApi.Common.Paginations.OffSet;
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
        : IQueryHandler<CursorRequest<Response>, CursorPagedList<Response>>
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

        public async Task<Result<CursorPagedList<Response>>> Handle(
            CursorRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var cursorData = CursorHelper.Decode(query.Cursor);

            var isAll = false;
            if (query.search is not null)
            {
                isAll = query.search.Contains("all", StringComparison.OrdinalIgnoreCase);
            }

            var baseQuery = _db.Notifications
                .Where(e => e.IsRead == isAll);

            // Step 3: Apply cursor filter
            // For "next" (older items): CreatedOnUtc < cursor OR (same time AND Id > cursor)
            // We use Id > cursor for tie-breaking (arbitrary but consistent)
            if (cursorData is not null)
            {

                if (query.Direction == CursorDirection.Next)
                {
                    // Get items OLDER than cursor (going forward in desc list)
                    baseQuery = baseQuery.Where(e =>
                        e.CreatedOnUtc < cursorData.CreatedOnUtc ||
                        (e.CreatedOnUtc == cursorData.CreatedOnUtc
                        && e.Id.CompareTo(cursorData.Id) > 0));
                }
                else // "prev"
                {
                    // Get items NEWER than cursor (going backward)
                    baseQuery = baseQuery.Where(e =>
                        e.CreatedOnUtc > cursorData.CreatedOnUtc ||
                        (e.CreatedOnUtc == cursorData.CreatedOnUtc
                        && e.Id.CompareTo(cursorData.Id) < 0));
                }
            }

            // Step 4: Order and fetch one extra item to detect hasNextPage
            IQueryable<Notification> orderedQuery;
            if (query.Direction == CursorDirection.Next)
            {
                orderedQuery = baseQuery
                    .OrderByDescending(e => e.CreatedOnUtc)
                    .ThenBy(e => e.Id);
            }
            else
            {
                // For "prev", reverse order then flip results
                orderedQuery = baseQuery
                    .OrderBy(e => e.CreatedOnUtc)
                    .ThenByDescending(e => e.Id);
            }

            // Fetch pageSize + 1 to check if there are more items
            var notifications = await orderedQuery
                .Take(query.PageSize + 1)
                .Select(e => new Response(e.Id, e.Title, e.Body, e.CreatedOnUtc))
                .ToListAsync(cancellationToken);

            if (notifications.Count <= 0)
            {
                return Result<CursorPagedList<Response>>
                    .Failure(NotificationErrors.NotFound);
            }

            bool hasMore = notifications.Count > query.PageSize;

            if (hasMore)
                notifications = notifications.Take(query.PageSize).ToList();

            // For "prev" direction, reverse to maintain newest-first order
            if (query.Direction == CursorDirection.Prev)
                notifications.Reverse();

            // Step 6: Generate cursors
            string? nextCursor = null;
            string? previousCursor = null;

            if (notifications.Any())
            {
                var firstItem = notifications.First();
                var lastItem = notifications.Last();

                // NextCursor points to last item (to get older items)
                if (hasMore || query.Direction == CursorDirection.Prev)
                    nextCursor = CursorHelper.Encode(lastItem.CreatedOnUtc, lastItem.Id);

                // PreviousCursor points to first item (to get newer items)
                // Only if we're not at the very beginning
                if (cursorData is not null)
                    previousCursor = CursorHelper.Encode(firstItem.CreatedOnUtc, firstItem.Id);
            }

            bool hasNextPage = query.Direction == CursorDirection.Next ?
                hasMore : cursorData != null;
            bool hasPreviousPage = query.Direction == CursorDirection.Prev ?
                hasMore : cursorData != null;

            var pageSize = notifications.Count < query.PageSize
                ? notifications.Count : query.PageSize;

            // Step 7: Build response
            var response = CursorPagedList<Response>.Create(
                notifications,
                pageSize,
                hasNextPage,
                hasPreviousPage,
                nextCursor,
                previousCursor);

            return Result<CursorPagedList<Response>>.Success(response);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/notifications", [Authorize] async (
                [FromQuery] int? pageSize,
                [FromQuery] string? cursor,
                [FromQuery] string? direction,
                IQueryHandler<CursorRequest<Response>, CursorPagedList<Response>> handler,
                CancellationToken ct = default) =>
            {
                var query = CursorRequest<Response>.Create(pageSize, cursor, direction);
                var result = await handler.Handle(query, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Problem();
            })
            .WithTags($"{nameof(Notification)}s")
            .WithSummary("Get all notifications (cursor pagination)")
            .WithDescription("Retrieves unread notifications using cursor-based pagination. Pass 'cursor' from previous response to load more.");
        }
    }
}
