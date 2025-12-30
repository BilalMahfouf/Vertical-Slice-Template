using Carter;
using Carter.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken)
        : ICommand<Response>;
    public record Response(string Token, string RefreshToken);

    public sealed class RefreshTokenCommandHandler
        : ICommandHandler<RefreshTokenCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IJwtProvider _jwtProvider;

        public RefreshTokenCommandHandler(
            IApplicationDbContext db,
            IJwtProvider jwtProvider)
        {
            _db = db;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result<Response>> Handle(
            RefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            var session = await _db.UserSessions
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Token == command.RefreshToken);
            if (session is null)
            {
                return Result<Response>.Failure(UserErrors.InvalidCredentials);
            }
            if (session.ExpiresAt < DateTime.Now)
            {
                return Result<Response>.Failure(UserErrors.ExpiredRefreshToken);
            }
            var token = _jwtProvider.GenerateToken(session.User);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            session.Token = refreshToken;

            _db.UserSessions.Update(session);
            await _db.SaveChangesAsync();

            var response = new Response(token, refreshToken);
            return Result<Response>.Success(response);
        }
    }
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/refresh-token", async (
                [FromBody] RefreshTokenCommand command,
                [FromServices] ICommandHandler<RefreshTokenCommand, Response> handler,
                CancellationToken cancellationToken=default) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return Results.Ok();
            });
        }
    }
}

