using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IQuery<Response>;
    public record Response(
        Guid Id,
        string UserName,
        string Email,
        string FirstName,
        string LastName);

    public class GetUserByIdQueryHandler
        : IQueryHandler<GetUserByIdQuery, Response>
    {
        private readonly IApplicationDbContext _db;

        public GetUserByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Response>> Handle(
            GetUserByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var response = await _db.Users
                .Where(u => u.Id == query.UserId)
                .Select(u => new Response(
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FirstName,
                    u.LastName))
                .AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            if (response is null)
            {
                return Result<Response>.Failure(
                    UserErrors.UserNotFound(query.UserId));
            }
            return Result<Response>.Success(response);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/{userId:guid}",
               [Authorize] async (Guid userId,
                IQueryHandler<GetUserByIdQuery, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetUserByIdQuery(userId);
                var result = await handler.Handle(
                        query, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Problem();
            })
            .WithTags($"{nameof(User)}s")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves user information by their unique identifier. Returns user ID, email, and full name.");
        }
    }
}
