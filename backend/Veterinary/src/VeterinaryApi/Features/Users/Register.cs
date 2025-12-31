using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public static class Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string UserName,
        string FirstName,
        string LastName) : ICommand;

    public class RegisterCommandHandler
        : ICommandHandler<RegisterCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public RegisterCommandHandler(
            IApplicationDbContext db,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result> Handle(
            RegisterCommand command,
            CancellationToken cancellationToken = default)
        {
            var hashPassword = _passwordHasher.Hash(command.Password);

            var user = User.Register(
                command.UserName,
                command.FirstName,
                command.LastName,
                command.Email,
                hashPassword);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", async (
                RegisterCommand command,
                ICommandHandler<RegisterCommand> hander,
                CancellationToken cancellationToken = default) =>
            {
                var result = await hander.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Ok() : result.Problem();
            }).WithTags("Authentication");
        }
    }
}
