using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Common.Util;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

public static class CreateVisitReceipt
{
    public sealed record Command(Guid VisitId) : ICommand<Common.PdfResposeData>;

    public sealed record ReceiptData(
        string ClinicName,
        string ClinicPhone,
        string ClinicAddress,
        string DoctorName,
        string InvoiceNumber,
        string InvoiceDate,
        string ClientName,
        string? ClientPhone,
        string VisitDescription,
        string Amount,
        string Status,
        bool IsPaid);

    public sealed class CommandHandler : ICommandHandler<Command, Common.PdfResposeData>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public CommandHandler(IApplicationDbContext db, ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        public async Task<Result<Common.PdfResposeData>> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var clinic = await _db.Clinics
                .Where(u => u.DoctorId == _currentTenant.UserId!.Value)
                .Select(e => new
                {
                    DoctorName = e.Doctor.FullName,
                    ClinicName = e.Name,
                    ClinicPhone = e.Phone,
                    ClinicAddress = e.Address,
                }).FirstOrDefaultAsync(cancellationToken);

            if (clinic is null)
            {
                return Result<Common.PdfResposeData>
                    .Failure(ClinicErrors.ClinicsNotFound);
            }

            var visit = await _db.Visits
                .Where(e => e.Id == command.VisitId)
                .Select(e => new
                {
                    e.Id,
                    e.CreatedOnUtc,
                    e.PaymentAmount,
                    e.PaymentStatus,
                    e.Notes,
                    e.VisitType,
                    PetName = e.Animal.Name,
                    OwnerName = e.Owner.FullName,
                    OwnerPhone = e.Owner.Phone,
                }).FirstOrDefaultAsync(cancellationToken);

            if (visit is null)
            {
                return Result<Common.PdfResposeData>
                    .Failure(VisitErrors.VisitNotFound(command.VisitId));
            }

            var visitDate = visit.CreatedOnUtc.ToLocalTime();
            var invoiceNumber = $"FAC-{visitDate:yyyy}-{visit.Id.ToString()[..6].ToUpper()}";
            var invoiceDate = Utility.GetDateInFrench(visitDate);

            var statusText = visit.PaymentStatus switch
            {
                PaymentStatus.Paid => "Payé",
                PaymentStatus.PartiallyPaid => "Partiellement Payé",
                PaymentStatus.Refunded => "Remboursé",
                _ => "Non Payé"
            };

            var description = !string.IsNullOrWhiteSpace(visit.Notes)
                ? visit.Notes
                : $"{visit.VisitType} — {visit.PetName}";

            var receiptData = new ReceiptData(
                clinic.ClinicName,
                clinic.ClinicPhone ?? "",
                clinic.ClinicAddress ?? "",
                clinic.DoctorName,
                invoiceNumber,
                invoiceDate,
                visit.OwnerName,
                visit.OwnerPhone,
                description,
                visit.PaymentAmount.ToString("N0"),
                statusText,
                visit.PaymentStatus == PaymentStatus.Paid);

            var templateContent = string.Empty;
            var assembly = typeof(CreateVisitReceipt).Assembly;
            var resourceName = "VeterinaryApi.Features.Visits.ReceiptTemplate.hbs";
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream is not null)
            {
                using var reader = new StreamReader(resourceStream);
                templateContent = await reader.ReadToEndAsync(cancellationToken);
            }
            else
            {
                var templatePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Features", "Visits", "ReceiptTemplate.hbs");
                templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
            }

            var template = Handlebars.Compile(templateContent);
            var html = template(receiptData);

            var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
            if (string.IsNullOrEmpty(executablePath))
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
            }

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = string.IsNullOrEmpty(executablePath) ? null : executablePath,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            return Result<Common.PdfResposeData>
                .Success(new Common.PdfResposeData(pdfBytes, $"facture-{invoiceNumber}.pdf"));
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/visits/{visitId:guid}/receipt", async (
                Guid visitId,
                [FromServices] ICommandHandler<Command, Common.PdfResposeData> handler,
                CancellationToken ct = default) =>
            {
                var command = new Command(visitId);
                var result = await handler.Handle(command, ct);

                if (result.IsSuccess)
                {
                    return Results.File(
                        result.Value.PdfBytes,
                        "application/pdf",
                        result.Value.FileName);
                }

                return result.Problem();
            }).WithTags("Visits")
            .RequireAuthorization();
        }
    }
}
