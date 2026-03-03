using Resend;
using VeterinaryApi.Common.Abstracions.Emails;
using VeterinaryApi.Common.Errors;
using VeterinaryApi.Common.Results;

namespace VeterinaryApi.Infrastructure.Services.Notifications;

public sealed class ResendEmailService(
    IResend resend,
    ILogger<ResendEmailService>logger) : IEmailService
{
    public async Task<Result> SendEmailAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken)
    {

        try
        {
            logger.LogInformation("sending email");
            var message = new EmailMessage();
            message.From = "billelgamer3@gmail.com";
            message.Subject = request.Subject;
            message.To = request.To;
            message.HtmlBody = request.Body;

            await resend.EmailSendAsync(message, cancellationToken);
            logger.LogInformation("email send succsessfuly");
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception in the class {nameof(ResendEmailService)}" +
                $" in the function {nameof(SendEmailAsync)}.\n" +
                $"Ex: {ex}");
            return Result.Failure(Error.Failure("", ""));
        }
    }
}
