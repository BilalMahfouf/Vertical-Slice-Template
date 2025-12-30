using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class ResetPassword
{
    public sealed record ResetPasswordCommand(
        string Password,
        string ConfirmPassword,
        string Token,
        string Email) : ICommand;

    public sealed class ResetPasswordCommandHandler
        : ICommandHandler<ResetPasswordCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(
            IApplicationDbContext db,
            IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> Handle(
            ResetPasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Email == command.Email,
                cancellationToken);
            if (user is null)
            {
                return Result.Failure(UserErrors.UserNotFound(command.Email));
            }
            if (!user.Sessions.Any(u => u.Token == command.Token &&
            u.ExpiresAt > DateTime.UtcNow
            && u.TokenType == UserSessionTokenType.ResetPassword))
            {
                return Result.Failure(UserErrors.InvalidCredentials);
            }
            var newPasswordHash = _passwordHasher.Hash(command.Password);
            user.PasswordHash = newPasswordHash;
            _db.Users.Update(user);

            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
