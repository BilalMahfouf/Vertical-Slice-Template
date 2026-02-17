using HandlebarsDotNet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Errors;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Prescriptions;

public static class CreatePricription
{
    public sealed record Request(
         string? PatientFullName = null,
        string? AnimalType = null,
        string? PatientAge = null,
        string? PatientWeight = null,
        List<string>? Medicines = null
            );
    public sealed record Command(
        bool isEmpty = false,
        string? PatientFullName = null,
        string? AnimalType = null,
        string? PatientAge = null,
        string? PatientWeight = null,
        List<string>? Medicines = null
    ) : ICommand<Response>;

    public sealed record PdfData(
        string CabinetName,
        string DoctorName,
        string? Phone = null,
        string? PatientFullName = null,
        string? AnimalType = null,
        string? PatientAge = null,
        string? PatientWeight = null,
        string? Date = null,
        List<string>? Medicines = null
        );


    public sealed record Response(byte[] PdfData);

    private static string GetDateInFrench(DateTime date)
    {
        var month = date.Month switch
        {
            1 => "Janvier",
            2 => "Février",
            3 => "Mars",
            4 => "Avril",
            5 => "Mai",
            6 => "Juin",
            7 => "Juillet",
            8 => "Août",
            9 => "Septembre",
            10 => "Octobre",
            11 => "Novembre",
            12 => "Décembre",
            _ => ""
        };
        var stringDate = $"{date.Day} / {month} / {date.Year}";
        return stringDate;
    }
    public sealed class CommandHandler : ICommandHandler<Command, Response>
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly IApplicationDbContext _db;

        public CommandHandler(
            ICurrentTenant currentTenant,
            IApplicationDbContext db)
        {
            _currentTenant = currentTenant;
            _db = db;
        }

        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var data = await _db.Clinics
                .ForTenant(_currentTenant.UserId!.Value)
                .Select(e => new
                {
                    DoctorName = e.Doctor.FullName,
                    ClinicName = e.Name,
                    ClinicPhone = e.Phone,
                }).FirstOrDefaultAsync(cancellationToken);
            if (data is null)
            {
                return Result<Response>
                    .Failure(ClinicErrors.ClinicsNotFound);
            }

            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
               "Features", "Prescriptions", "PrescriptionTemplate.hbs");
            var templateContent = await File.ReadAllTextAsync(templatePath);

            var template = Handlebars.Compile(templateContent);
            var date = GetDateInFrench(DateTime.Now);
            PdfData pdfParam;
            if (command.isEmpty)
            {
                pdfParam = new PdfData(
                    data.ClinicName, data.DoctorName, data.ClinicPhone);
            }
            else
            {
                pdfParam = new PdfData(
                    data.ClinicName,
                    data.DoctorName,
                    data.ClinicPhone,
                    command.PatientFullName,
                    command.AnimalType,
                    command.PatientAge,
                    command.PatientWeight,
                    date,
                    command.Medicines);
            }
            var html = template(pdfParam);
            if (html is null)
            {

            }
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");

            var pdfData = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });
            return Result<Response>.Success(new Response(pdfData));
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/prescriptions", async (
                 CreatePricription.Request request,
                [FromServices] ICommandHandler<Command, Response> handler,
                CancellationToken ct = default) =>
            {
                var command = new Command(
                    false,
                    request.PatientFullName,
                    request.AnimalType,
                    request.PatientAge,
                    request.PatientWeight,
                    request.Medicines);
                var result = await handler.Handle(command, ct);
                if (result.IsSuccess)
                {
                    return Results.File(
                        result.Value.PdfData,
                        "application/pdf",
                        "prescription.pdf");
                }
                return result.Problem();
            }).WithTags("Prescriptions")
            .RequireAuthorization();
        }
    }

    public sealed class Endpoint1 : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/prescriptions/empty", async (
                ICommandHandler<CreatePricription.Command, Response> handler,
                CancellationToken ct = default) =>
            {
                var command = new Command(true);
                var result = await handler.Handle(command, ct);
                if (result.IsSuccess)
                {
                    return Results.File(
                        result.Value.PdfData,
                        "application/pdf",
                        "prescription.pdf");
                }
                return result.Problem();
            }).WithTags("Prescriptions")
            .RequireAuthorization();
        }
    }
}
