using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.Abstracions.Emails;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Common.Util;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class ForgetPassword
{
    public record ForgetPasswordCommand(string Email, string ClientUri)
        : ICommand;
    public record Response();

    public class ForgetPasswordCommandHandler
        : ICommandHandler<ForgetPasswordCommand>
    {

        private readonly IApplicationDbContext _db;
        private readonly IJwtProvider _jwtProvider;
        private readonly IEmailService _emailService;

        public ForgetPasswordCommandHandler(
            IApplicationDbContext db,
            IJwtProvider jwtProvider,
            IEmailService emailService)
        {
            _db = db;
            _jwtProvider = jwtProvider;
            _emailService = emailService;
        }

        public async Task<Result> Handle(
            ForgetPasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users
               .FirstOrDefaultAsync(e => e.Email == command.Email,
               cancellationToken);
            if (user is null)
            {
                return Result<Response>
                    .Failure(UserErrors.UserNotFound(command.Email));
            }
            var token = _jwtProvider.GenerateToken(user);
            var userSession = new UserSession()
            {
                UserId = user.Id,
                Token = token,
                TokenType = UserSessionTokenType.ResetPassword,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            _db.UserSessions.Add(userSession);
            await _db.SaveChangesAsync(cancellationToken);
            var link = Utility.GenerateResponseLink(command.Email, token
                , command.ClientUri);
            var body = $@"
                            <p>Click here to reset your password:</p>
                            <a href=""{link}"">Reset Password</a>";

            var message = new SendEmailRequest(user.Email, "Reset Password", body);
            await _emailService.SendEmailAsync(message, cancellationToken);
            return Result.Success;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/forget-password", async (
                ForgetPasswordCommand command,
                ICommandHandler<ForgetPasswordCommand> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return Results.Ok();
            });
        }
    }
}
