using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class Login
{
    public record Response(string Token, string RefreshToken);
    public record LoginCommand(string Email, string Password)
        : ICommand<Response>;

    internal class LoginCommandHandler : ICommandHandler<LoginCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHahser;
        private readonly IJwtProvider _jwtProvider;

        public LoginCommandHandler(
            IApplicationDbContext db,
            IPasswordHasher passwordHahser,
            IJwtProvider jwtProvider)
        {
            _db = db;
            _passwordHahser = passwordHahser;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result<Response>> Handle(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(
                e => e.Email == command.Email,
                cancellationToken);

            if(user is null)
            {
                var error = UserErrors.UserNotFound(command.Email);
                return Result<Response>.Failure(error);
            }
            bool validPassword = _passwordHahser
                .Verify(command.Password, user.PasswordHash);
            if(!validPassword)
            {
                return Result<Response>.Failure(
                    UserErrors.InvalidCredentials());
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

            var response = new Response(token, refreshToken);
            return Result<Response>.Success(response);
        }
    }

}
