using Carter;
using Microsoft.JSInterop.Infrastructure;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Infrastructure;
using DotNetEnv;
using Scalar.AspNetCore;
using VeterinaryApi.Common.Exceptions;
using FluentValidation;
using VeterinaryApi.Common.Extensions;
using VeterinaryApi.Infrastructure.Notifications;


Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();




// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddValidatorsFromAssemblyContaining
    <VeterinaryApi.Features.Animals.CreateAnimal.Validator>(ServiceLifetime.Singleton);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();



builder.Services.Scan(scan => scan.FromAssembliesOf(typeof(Program))
    .AddClasses(classes => classes
        .AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime()

    .AddClasses(classes => classes.
        AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime()

    .AddClasses(classes => classes
        .AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
    .AsImplementedInterfaces()
        .WithScopedLifetime());

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCarter();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

builder.Services.AddAuthorization();



var app = builder.Build();

// Global API prefix

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.ApplyMigrations();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notification");

var api = app.MapGroup("/api/v1");
api.MapCarter();



app.Run();


