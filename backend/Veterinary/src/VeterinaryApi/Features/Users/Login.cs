using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class Login
{
    public record Response(string Token);
    public record LoginCommand(string Email, string Password)
        : ICommand<Response>;

    public class LoginCommandHandler : ICommandHandler<LoginCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHahser;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginCommandHandler(
            IApplicationDbContext db,
            IPasswordHasher passwordHahser,
            IJwtProvider jwtProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _passwordHahser = passwordHahser;
            _jwtProvider = jwtProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<Response>> Handle(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(
                e => e.Email == command.Email,
                cancellationToken);

            if (user is null)
            {
                var error = UserErrors.UserNotFound(command.Email);
                return Result<Response>.Failure(error);
            }
            bool validPassword = _passwordHahser
                .Verify(command.Password, user.PasswordHash);
            if (!validPassword)
            {
                return Result<Response>.Failure(
                    UserErrors.InvalidCredentials);
            }
            var token = _jwtProvider.GenerateToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            var userSession = new UserSession
            {
                UserId = user.Id,
                Token = refreshToken,
                TokenType = UserSessionTokenType.Refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };
            _db.UserSessions.Add(userSession);
            await _db.SaveChangesAsync(cancellationToken);

            _httpContextAccessor.HttpContext!.Response
                            .Cookies.Append(
                "refreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = userSession.ExpiresAt,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                });
            var response = new Response(token);
            return Result<Response>.Success(response);
        }


    }

    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/login", async (
                [FromBody] Login.LoginCommand request,
                [FromServices] ICommandHandler<LoginCommand, Response> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(request, cancellationToken);
                return result.IsSuccess ? Results.Ok(new
                {
                    result.Value
                }) : result.Problem();
            }).WithTags("Authentication");
        }
    }
}
