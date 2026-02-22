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

/// <summary>
/// Vertical slice for generating a veterinary prescription as a PDF document.
/// Uses a Handlebars.NET template and PuppeteerSharp (headless Chromium) for rendering.
/// Note: filename is <c>CreatePricription.cs</c> — a typo for ‘Prescription’.
/// </summary>
public static class CreatePricription
{
    /// <summary>HTTP request body DTO for a filled prescription. All fields are optional.</summary>
    public sealed record Request(
         string? PatientFullName = null,
        string? AnimalType = null,
        string? PatientAge = null,
        string? PatientWeight = null,
        List<string>? Medicines = null
            );

    /// <summary>
    /// Command that drives PDF generation.
    /// When <paramref name="isEmpty"/> is <c>true</c> an empty template is rendered (no patient data).
    /// </summary>
    public sealed record Command(
        bool isEmpty = false,
        string? PatientFullName = null,
        string? AnimalType = null,
        string? PatientAge = null,
        string? PatientWeight = null,
        List<string>? Medicines = null
    ) : ICommand<Response>;

    /// <summary>Data model passed to the Handlebars template during rendering.</summary>
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

    /// <summary>Response DTO wrapping the raw PDF byte array.</summary>
    /// <param name="PdfData">The generated prescription PDF as a raw byte array.</param>
    public sealed record Response(byte[] PdfData);

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a French-language date string
    /// in the format <c>"day / MonthName / year"</c>.
    /// </summary>
    /// <param name="date">The date to convert.</param>
    /// <returns>A French-formatted date string, e.g. <c>"5 / Mars / 2025"</c>.</returns>
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
    /// <summary>
    /// Handles the <see cref="Command"/> by:
    /// <list type="number">
    ///   <item>Loading clinic and doctor details from the tenant's record.</item>
    ///   <item>Loading the Handlebars template from an embedded resource, falling back to the file system.</item>
    ///   <item>Rendering the template with <see cref="PdfData"/>.</item>
    ///   <item>Launching a headless Chromium browser via PuppeteerSharp and exporting an A4 PDF.</item>
    /// </list>
    /// </summary>
    public sealed class CommandHandler : ICommandHandler<Command, Response>
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with tenant context and database services.</summary>
        public CommandHandler(
            ICurrentTenant currentTenant,
            IApplicationDbContext db)
        {
            _currentTenant = currentTenant;
            _db = db;
        }

        /// <summary>
        /// Generates the prescription PDF. Returns <c>ClinicErrors.ClinicsNotFound</c> if the tenant
        /// has no clinic on record.
        /// </summary>
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

            var templateContent = string.Empty;
            var assembly = typeof(CreatePricription).Assembly;
            var resourceName = "VeterinaryApi.Features.Prescriptions.PrescriptionTemplate.hbs";
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream is not null)
            {
                using var reader = new StreamReader(resourceStream);
                templateContent = await reader.ReadToEndAsync(cancellationToken);
            }
            else
            {
                // fallback for local development
                var templatePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Features", "Prescriptions", "PrescriptionTemplate.hbs");
                templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
            }

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
            var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
            if (string.IsNullOrEmpty(executablePath))
            {
                // Local dev: download Chromium if not already present
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

            var pdfData = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });
            return Result<Response>.Success(new Response(pdfData));
        }
    }
    /// <summary>Carter endpoint that maps <c>POST /prescriptions</c>. Accepts a <see cref="Request"/> body and streams the PDF file. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the create-prescription route.</summary>
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

    /// <summary>Carter endpoint that maps <c>GET /prescriptions/empty</c>. Returns an empty (unfilled) prescription template PDF. Requires authorization.</summary>
    public sealed class Endpoint1 : IEndpoint
    {
        /// <summary>Registers the empty-prescription-template route.</summary>
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
