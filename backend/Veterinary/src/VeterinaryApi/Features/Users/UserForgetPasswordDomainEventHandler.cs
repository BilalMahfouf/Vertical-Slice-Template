using VeterinaryApi.Common.Abstracions.Emails;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Util;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Features.Users;

public sealed class UserForgetPasswordDomainEventHandler(
    IEmailService emailService)
    : IDomainEventHandler<UserForgetPasswordDomainEvent>
{

    public async Task Handle(
        UserForgetPasswordDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var link = Utility.GenerateResponseLink(
            domainEvent.Email, domainEvent.Token, domainEvent.ClientUri);
        var body = $@"
                            <p>Click here to reset your password:</p>
                            <a href=""{link}"">Reset Password</a>";

        var message = new SendEmailRequest(domainEvent.Email, "Reset Password", body);
        await emailService.SendEmailAsync(message, cancellationToken);

    }
}
