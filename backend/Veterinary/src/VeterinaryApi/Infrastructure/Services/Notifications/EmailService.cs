using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VeterinaryApi.Common.Abstracions.Emails;
using VeterinaryApi.Common.Errors;
using VeterinaryApi.Common.Results;

namespace VeterinaryApi.Infrastructure.Services.Notifications;

internal class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public EmailService(IOptions<EmailOptions> options)
    {
        _emailOptions = options.Value;
    }

    public async Task<Result> SendEmailAsync(SendEmailRequest request
        , CancellationToken cancellationToken)
    {
        try
        {

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailOptions.Email));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = request.Body
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailOptions.Host, _emailOptions.Port
                , SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_emailOptions.Email
                , _emailOptions.Password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            var error = Error.Failure(
                "Email.Exception", ex.Message);
            return Result.Failure(error);
        }
    }
}